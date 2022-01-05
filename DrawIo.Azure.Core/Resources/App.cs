using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class App : AzureResource, ICanBeExposedByPrivateEndpoints
{
    private static readonly (HttpMethod, string) ConfigApiEndpoint = (HttpMethod.Post, "config/appSettings/list");
    private VNetIntegration? _azureVNetIntegrationResource;
    public override bool FetchFull => true;
    public string Kind { get; set; }
    public AppProperties Properties { get; set; }
    public Identity? Identity { get; set; }
    public override string ApiVersion => "2021-01-15";
    public override string Image => "img/lib/azure2/app_services/App_Services.svg";

    public override HashSet<(HttpMethod, string)> AdditionalResources => new() { ConfigApiEndpoint };

    public (string storageName, string storageSuffix)[] ConnectedStorageAccounts { get; set; }

    public string? AppInsightsKey { get; set; }

    public bool AccessedViaPrivateEndpoint(PrivateEndpoint privateEndpoint)
    {
        return Properties.PrivateEndpoints.Contains(privateEndpoint.Id.ToLowerInvariant());
    }

    public override async Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        Properties = full["properties"].ToObject<AppProperties>();

        Properties.PrivateEndpoints =
            full["properties"]["privateEndpointConnections"]
                .Select(x => x["properties"]["privateEndpoint"].Value<string>("id").ToLowerInvariant())
                .ToArray();


        var config = additionalResources[ConfigApiEndpoint.Item2];
        var appSettings = config["properties"]!.ToObject<Dictionary<string, object>>()!;

        if (appSettings.ContainsKey("APPINSIGHTS_INSTRUMENTATIONKEY"))
        {
            AppInsightsKey = (string)appSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];
        }

        ConnectedStorageAccounts = appSettings
            .Values
            .OfType<string>()
            .Where(appSetting => appSetting.Contains("DefaultEndpointsProtocol") &&
                                 appSetting.Contains("AccountName") &&
                                 appSetting.Contains("EndpointSuffix"))
            .Select(x =>
            {
                var parts = x!.Split(';')
                    .Select(x =>
                        new KeyValuePair<string, string>(x.Split('=')[0].ToLowerInvariant(),
                            x.Split('=')[1].ToLowerInvariant()))
                    .ToDictionary(x => x.Key, x => x.Value);

                return (parts["accountname"], "." + parts["endpointsuffix"]);
            })
            .ToArray();
    }

    public override IEnumerable<AzureResource> DiscoverNewNodes()
    {
        if (Properties.VirtualNetworkSubnetId != null)
        {
            _azureVNetIntegrationResource = new VNetIntegration($"{Id}.vnetintegration", Properties.VirtualNetworkSubnetId);
            yield return _azureVNetIntegrationResource;
        }
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        if (AppInsightsKey != null)
        {
            var appInsights = allResources.OfType<AppInsights>()
                .SingleOrDefault(x => x.InstrumentationKey == AppInsightsKey);
            if (appInsights != null) CreateFlowTo(appInsights);
        }

        foreach (var storageAccount in ConnectedStorageAccounts)
        {
            var storage = allResources.OfType<StorageAccount>()
                .SingleOrDefault(x => x.Name.ToLowerInvariant() == storageAccount.storageName);

            if (storage != null)
            {
                if (_azureVNetIntegrationResource != null)
                {
                    CreateFlowTo(_azureVNetIntegrationResource);
                }

                var flowSource = _azureVNetIntegrationResource as AzureResource ?? this;
                var privateEndpointConnection = storage.ExposedByPrivateEndpoints.SingleOrDefault(x =>
                    x.CustomHostNames.Any(x =>
                        x.StartsWith(storageAccount.storageName) && x.EndsWith(storageAccount.storageSuffix)));
                
                if (privateEndpointConnection != null)
                {
                    //connection hostname uses a private endpoint hostname.... Take a plunge and link the private endpoint instead:
                    flowSource.CreateFlowTo(privateEndpointConnection);
                }
                else
                {
                    flowSource.CreateFlowTo(storage);
                }
            }
        }
    }

    public class AppProperties
    {
        public string ServerFarmId { get; set; }
        public string[] PrivateEndpoints { get; set; }
        public string? VirtualNetworkSubnetId { get; set; }
    }
}