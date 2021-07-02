using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class App : AzureResource
    {
        public override bool IsSpecific => true;
        public override bool FetchFull => true;
        public string Kind { get; set; }
        public AppProperties Properties { get; set; }
        public Identity? Identity { get; set; }
        public override string ApiVersion => "2021-01-15";
        public override string Image => "img/lib/azure2/app_services/App_Services.svg";

        public class AppProperties
        {
            public string ServerFarmId { get; set; }
            public string[] PrivateEndpoints { get; set; }
        }

        public override void Enrich(JObject full)
        {
            Properties = full["properties"].ToObject<AppProperties>();
            Properties.PrivateEndpoints =
                full["properties"]["privateEndpointConnections"]
                    .Select(x => x["properties"]["privateEndpoint"].Value<string>("id").ToLowerInvariant())
                    .ToArray();
        }

        public bool AccessedViaPrivateEndpoint(PrivateEndpoint privateEndpoint)
        {
            return Properties.PrivateEndpoints.Contains(privateEndpoint.Id.ToLowerInvariant());
        }
    }
}