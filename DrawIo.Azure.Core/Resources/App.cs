using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Msagl.Core.Layout;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class App : AzureResource
    {
        private static readonly (HttpMethod, string) ConfigApiEndpoint = (HttpMethod.Post, "config/appSettings/list");
        public override bool IsSpecific => true;
        public override bool FetchFull => true;
        public string Kind { get; set; }
        public AppProperties Properties { get; set; }
        public Identity? Identity { get; set; }
        public override string ApiVersion => "2021-01-15";
        public override string Image => "img/lib/azure2/app_services/App_Services.svg";

        public override HashSet<(HttpMethod,string)> AdditionalResources => new() {ConfigApiEndpoint};

        public class AppProperties
        {
            public string ServerFarmId { get; set; }
            public string[] PrivateEndpoints { get; set; }
        }

        public override async Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
        {
            Properties = full["properties"].ToObject<AppProperties>();
            Properties.PrivateEndpoints =
                full["properties"]["privateEndpointConnections"]
                    .Select(x => x["properties"]["privateEndpoint"].Value<string>("id").ToLowerInvariant())
                    .ToArray();

            var config = additionalResources[ConfigApiEndpoint.Item2];
            AppInsightsKey = config["properties"]!.Value<string>("APPINSIGHTS_INSTRUMENTATIONKEY");
            var functionsStorage = config["properties"]!.Value<string>("AzureWebJobsStorage");
            if (functionsStorage != null)
            {
                FunctionStorageAccount = functionsStorage!.Split(';')
                    .Select(x => new KeyValuePair<string, string>(x.Split('=')[0].ToLowerInvariant(), x.Split('=')[1].ToLowerInvariant()))
                    .ToDictionary(x => x.Key, x => x.Value)
                    ["accountname"];
            }
        }

        public string? FunctionStorageAccount { get; set; }

        public string? AppInsightsKey { get; set; }

        public bool AccessedViaPrivateEndpoint(PrivateEndpoint privateEndpoint)
        {
            return Properties.PrivateEndpoints.Contains(privateEndpoint.Id.ToLowerInvariant());
        }

        public override void Link(IEnumerable<AzureResource> allResources, GeometryGraph graph)
        {
            base.Link(allResources, graph);
            if (AppInsightsKey != null)
            {
                var appInsights = allResources.OfType<AppInsights>().SingleOrDefault(x => x.InstrumentationKey == AppInsightsKey);
                if (appInsights != null)
                {
                    Link(appInsights, graph);
                }
            }

            if (FunctionStorageAccount != null)
            {
                var storage = allResources.OfType<StorageAccount>().SingleOrDefault(x => x.Name.ToLowerInvariant() == FunctionStorageAccount);
                if (storage != null)
                {
                    Link(storage, graph);
                }
            }
        }
    }
}