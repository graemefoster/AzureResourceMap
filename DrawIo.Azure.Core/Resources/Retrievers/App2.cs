using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Diagrams;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class App2 : AzureResource, ICanBeExposedByPrivateEndpoints
{

    private VNetIntegration? _azureVNetIntegrationResource;

    public AppProperties Properties { get; set; }

    public Identity? Identity { get; set; }
    public override string Image => "img/lib/azure2/app_services/App_Services.svg";

    public (string storageName, string storageSuffix)[] ConnectedStorageAccounts { get; set; }

    public string? AppInsightsKey { get; set; }

    public bool AccessedViaPrivateEndpoint(PrivateEndpoint privateEndpoint)
    {
        return Properties.PrivateEndpoints.Contains(privateEndpoint.Id.ToLowerInvariant());
    }

    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new IgnoreNodeBuilder(this);
    }

    public override IEnumerable<AzureResource> DiscoverNewNodes()
    {
        if (Properties.VirtualNetworkSubnetId != null)
        {
            _azureVNetIntegrationResource =
                new VNetIntegration($"{Id}.vnetintegration", Properties.VirtualNetworkSubnetId);
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

    public class AppProperties
    {
        public string ServerFarmId { get; set; }
        public string[] PrivateEndpoints { get; set; }
        public string? VirtualNetworkSubnetId { get; set; }
    }
}