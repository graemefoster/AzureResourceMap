using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Diagrams;
using DrawIo.Azure.Core.Resources.Retrievers;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class App : AzureResource, ICanBeExposedByPrivateEndpoints
{
    private VNetIntegration? _azureVNetIntegrationResource;
    public string? ServerFarmId { get; set; }
    public string[] PrivateEndpoints { get; set; }
    public string? VirtualNetworkSubnetId { get; set; }
    public Identity? Identity { get; set; }
    public override string Image => "img/lib/azure2/app_services/App_Services.svg";

    public (string storageName, string storageSuffix)[] ConnectedStorageAccounts { get; set; }

    public string? AppInsightsKey { get; set; }

    public bool AccessedViaPrivateEndpoint(PrivateEndpoint privateEndpoint)
    {
        return PrivateEndpoints.Contains(privateEndpoint.Id.ToLowerInvariant());
    }

    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new AppServicePlanAppNodeBuilder(this);
    }

    public override async Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        VirtualNetworkSubnetId = full["properties"]!["virtualNetworkSubnetId"]?.Value<string>();
        ServerFarmId = full["properties"]!["serverFarmId"]?.Value<string>();

        PrivateEndpoints =
            full["properties"]!["privateEndpointConnections"]?
                .Select(x => x["properties"]["privateEndpoint"].Value<string>("id").ToLowerInvariant())
                .ToArray();


        var config = additionalResources[AppResourceRetriever.ConfigAppSettingsList];
        var appSettings = config["properties"]!.ToObject<Dictionary<string, object>>()!;

        if (appSettings.ContainsKey("APPINSIGHTS_INSTRUMENTATIONKEY"))
            AppInsightsKey = (string)appSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];

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
        if (VirtualNetworkSubnetId != null)
        {
            _azureVNetIntegrationResource =
                new VNetIntegration($"{Id}.vnetintegration", VirtualNetworkSubnetId);
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
                if (_azureVNetIntegrationResource != null) CreateFlowTo(_azureVNetIntegrationResource);

                var flowSource = _azureVNetIntegrationResource as AzureResource ?? this;
                var privateEndpointConnection = storage.ExposedByPrivateEndpoints.SingleOrDefault(x =>
                    x.CustomHostNames.Any(x =>
                        x.StartsWith(storageAccount.storageName) && x.EndsWith(storageAccount.storageSuffix)));

                if (privateEndpointConnection != null)
                    //connection hostname uses a private endpoint hostname.... Take a plunge and link the private endpoint instead:
                    flowSource.CreateFlowTo(privateEndpointConnection);
                else
                    flowSource.CreateFlowTo(storage);
            }
        }
    }
}