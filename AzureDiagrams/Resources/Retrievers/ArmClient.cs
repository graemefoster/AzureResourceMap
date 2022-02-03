using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core;
using DrawIo.Azure.Core.Resources.Retrievers.Custom;
using DrawIo.Azure.Core.Resources.Retrievers.Extensions;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers;

public class ArmClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenCredential _tokenCredential;

    public ArmClient(HttpClient httpClient, TokenCredential tokenCredential)
    {
        _httpClient = httpClient;
        _tokenCredential = tokenCredential;
    }

    public async Task<IEnumerable<AzureResource>> Retrieve(Guid subscriptionId, IEnumerable<string> resourceGroups)
    {
        var allDirectResources = await Task.WhenAll(resourceGroups.Select(rg =>
            _httpClient.GetAzResourceAsync<AzureList<JObject>>(
                $"/subscriptions/{subscriptionId}/resources?$filter=resourceGroup eq '{rg}'", "2020-10-01")));

        var allResources = allDirectResources.SelectMany(directResources =>
            directResources.Value.Select(GetResourceRetriever).Select(r => r.FetchResource(_httpClient)));

        return await Task.WhenAll(allResources);
    }

    private IRetrieveResource GetResourceRetriever(JObject basicAzureResourceInfo)
    {
        var type = basicAzureResourceInfo.Value<string>("type")!;
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(
            $"\tFound resource {type.ToLowerInvariant()}: {basicAzureResourceInfo.Value<string>("name")!}");
        Console.ResetColor();

        ResourceRetriever<AzureResource> Unknown()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\tCould not find typed wrapper for resource {type.ToLowerInvariant()}");
            Console.ResetColor();
            return new ResourceRetriever<AzureResource>(basicAzureResourceInfo);
        }

        return type.ToLowerInvariant() switch
        {
            "microsoft.network/virtualnetworks" => new ResourceRetriever<VNet>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-02-01"),
            "microsoft.network/privateendpoints" => new ResourceRetriever<PrivateEndpoint>(basicAzureResourceInfo,
                "2020-11-01", true),
            "microsoft.network/privatednszones" => new ResourceRetriever<PrivateDnsZone>(basicAzureResourceInfo,
                "2020-06-01", true),
            "microsoft.network/privatednszones/virtualnetworklinks" => new
                ResourceRetriever<DnsZoneVirtualNetworkLink>(basicAzureResourceInfo, "2020-06-01",
                    true),
            "microsoft.network/networkinterfaces" => new ResourceRetriever<Nic>(basicAzureResourceInfo, "2020-11-01",
                true),
            "microsoft.containerservice/managedclusters" => new ResourceRetriever<AKS>(basicAzureResourceInfo,
                "2020-11-01",
                extensions: new IResourceExtension[] { new DiagnosticsExtensions(), new ManagedIdentityExtension() }),
            "microsoft.containerregistry/registries" =>
                new ResourceRetriever<ACR>(basicAzureResourceInfo, "2021-09-01",
                    extensions: new IResourceExtension[]
                    {
                        new DiagnosticsExtensions(), new PrivateEndpointExtensions(), new ManagedIdentityExtension()
                    }),
            "microsoft.web/serverfarms" => new ResourceRetriever<ASP>(basicAzureResourceInfo,
                apiVersion: "2021-03-01", fetchFullResource: true, new[] { new DiagnosticsExtensions() }),
            "microsoft.web/sites" => new AppResourceRetriever(basicAzureResourceInfo),
            "microsoft.apimanagement/service" => new ApimServiceResourceRetriever(basicAzureResourceInfo),
            "microsoft.compute/virtualmachines" => new ResourceRetriever<VM>(basicAzureResourceInfo,
                "2021-07-01", true, extensions: new[] { new DiagnosticsExtensions() }),
            "microsoft.compute/disks" => new ResourceRetriever<Disk>(basicAzureResourceInfo),
            "microsoft.operationalinsights/workspaces" => new ResourceRetriever<LogAnalyticsWorkspace>(
                basicAzureResourceInfo, fetchFullResource: true, apiVersion: "2021-06-01"),
            "microsoft.insights/components" => new ResourceRetriever<AppInsights>(basicAzureResourceInfo, "2020-02-02",
                true),
            "microsoft.storage/storageaccounts" => new ResourceRetriever<StorageAccount>(basicAzureResourceInfo,
                "2021-08-01", true,
                extensions: new IResourceExtension[] { new DiagnosticsExtensions(), new PrivateEndpointExtensions() }),
            "microsoft.network/networksecuritygroups" => new ResourceRetriever<NSG>(basicAzureResourceInfo,
                fetchFullResource: true),
            "microsoft.network/publicipaddresses" => new ResourceRetriever<PIP>(basicAzureResourceInfo),
            "microsoft.managedidentity/userassignedidentities" => new ResourceRetriever<UserAssignedManagedIdentity>(
                basicAzureResourceInfo),
            "microsoft.keyvault/vaults" => new ResourceRetriever<KeyVault>(basicAzureResourceInfo,
                "2019-09-01", true,
                new IResourceExtension[] { new DiagnosticsExtensions(), new PrivateEndpointExtensions() }),
            "microsoft.sql/servers" => new ResourceRetriever<ManagedSqlServer>(basicAzureResourceInfo,
                apiVersion: "2021-02-01", fetchFullResource: false,
                new IResourceExtension[] { new DiagnosticsExtensions(), new PrivateEndpointExtensions() }),
            "microsoft.sql/servers/databases" => new ResourceRetriever<ManagedSqlDatabase>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-08-01-preview",
                extensions: new[] { new DiagnosticsExtensions() }),
            "microsoft.web/kubeenvironments" => new ResourceRetriever<ContainerAppEnvironment>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-03-01"),
            "microsoft.web/containerapps" => new ResourceRetriever<ContainerApp>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-03-01"),
            "microsoft.network/applicationgateways" => new ResourceRetriever<AppGateway>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-05-01", extensions: new[] { new DiagnosticsExtensions() }),
            "microsoft.botservice/botservices" => new ResourceRetriever<Bot>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-05-01-preview",
                extensions: new[] { new DiagnosticsExtensions() }),
            "microsoft.cognitiveservices/accounts" => new ResourceRetriever<CognitiveServices>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-10-01", extensions: new[] { new DiagnosticsExtensions() }),
            "microsoft.search/searchservices" => new ResourceRetriever<CognitiveSearch>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-04-01-preview",
                extensions: new IResourceExtension[] { new DiagnosticsExtensions(), new PrivateEndpointExtensions() }),
            "microsoft.documentdb/databaseaccounts" => new ResourceRetriever<CosmosDb>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-04-01-preview",
                extensions: new IResourceExtension[] { new DiagnosticsExtensions(), new PrivateEndpointExtensions() }),
            "microsoft.network/bastionhosts" => new ResourceRetriever<Bastion>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-05-01", extensions: new[] { new DiagnosticsExtensions() }),
            "microsoft.eventhub/namespaces" => new ResourceRetriever<EventHub>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-11-01",
                extensions: new IResourceExtension[] { new DiagnosticsExtensions(), new PrivateEndpointExtensions() }),
            "microsoft.network/azurefirewalls" => new ResourceRetriever<Firewall>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-05-01", extensions: new[] { new DiagnosticsExtensions() }),
            "microsoft.logic/workflows" => new ResourceRetriever<LogicApp>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2019-05-01"),
            "microsoft.web/connections" => new ResourceRetriever<LogicAppConnector>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2016-06-01"),
            "microsoft.devices/iothubs" => new ResourceRetriever<IotHub>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-07-02", extensions: new[] { new DiagnosticsExtensions() }),
            "microsoft.network/routetables" => new ResourceRetriever<UDR>(basicAzureResourceInfo),
            "microsoft.dbforpostgresql/servers" => new ResourceRetriever<ManagedSqlDatabase>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2017-12-01",
                extensions: new IResourceExtension[] { new DiagnosticsExtensions(), new PrivateEndpointExtensions() }),
            "microsoft.network/loadbalancers" => new ResourceRetriever<LoadBalancer>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-03-01", extensions: new[] { new DiagnosticsExtensions() }),
            "microsoft.web/hostingenvironments" => new ResourceRetriever<ASE>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-02-01", extensions: new[] { new DiagnosticsExtensions() }),
            "microsoft.servicebus/namespaces" => new ResourceRetriever<ServiceBus>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-06-01-preview",
                extensions: new IResourceExtension[] { new DiagnosticsExtensions(), new PrivateEndpointExtensions() }),
            "microsoft.eventgrid/topics" => new EventGridTopicRetriever(basicAzureResourceInfo),
            "microsoft.eventgrid/domains" => new EventGridDomainRetriever(basicAzureResourceInfo),
            "microsoft.datafactory/factories" => new AzureDataFactoryRetriever(basicAzureResourceInfo),
            //Would like to have a custom fetch, but the control plane is not all on ARM api surface, so if your synapse is private endpointed then I can't necessarily get much from it.
            "microsoft.synapse/workspaces" => new SynapseRetriever(basicAzureResourceInfo, _tokenCredential),
            "microsoft.synapse/workspaces/bigdatapools" => new ResourceRetriever<BigDataPool>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-06-01"),
            
            "microsoft.compute/virtualmachines/extensions" => new NoOpResourceRetriever(),
            "microsoft.alertsmanagement/smartdetectoralertrules" => new NoOpResourceRetriever(),
            "microsoft.compute/sshpublickeys" => new NoOpResourceRetriever(),
            "microsoft.insights/webtests" => new NoOpResourceRetriever(),
            "microsoft.insights/actiongroups" => new NoOpResourceRetriever(),
            "microsoft.network/firewallpolicies" => new NoOpResourceRetriever(),
            "microsoft.security/iotsecuritysolutions" => new NoOpResourceRetriever(),
            "microsoft.insights/autoscalesettings" => new NoOpResourceRetriever(),
            "microsoft.network/dnszones" => new NoOpResourceRetriever(),

            _ => Unknown()
        };
    }

    internal class AzureList<T>
    {
        public string? NextLink { get; set; }
        public T[] Value { get; set; } = default!;
    }
}