using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers;

public class ArmClient
{
    private readonly HttpClient _httpClient;

    public ArmClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<AzureResource>> Retrieve(string subscriptionId, IEnumerable<string> resourceGroups)
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
        Console.WriteLine(type);
        return type.ToLowerInvariant() switch
        {
            "microsoft.network/virtualnetworks" => new ResourceRetriever<VNet>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-02-01"),
            "microsoft.network/privateendpoints" => new ResourceRetriever<PrivateEndpoint>(basicAzureResourceInfo,
                "2020-11-01", true),
            "microsoft.network/privatednszones" => new ResourceRetriever<PrivateDnsZone>(basicAzureResourceInfo,
                "2020-06-01", true),
            "microsoft.network/privatednszones/virtualnetworklinks" => new
                ResourceRetriever<PrivateDnsZoneVirtualNetworkLink>(basicAzureResourceInfo, "2020-06-01",
                    true),
            "microsoft.network/networkinterfaces" => new ResourceRetriever<Nic>(basicAzureResourceInfo, "2020-11-01",
                true),
            "microsoft.containerservice/managedclusters" => new ResourceRetriever<AKS>(basicAzureResourceInfo,
                "2020-11-01"),
            "microsoft.containerregistry/registries" =>
                new ResourceRetriever<ACR>(basicAzureResourceInfo, "2020-11-01"),
            "microsoft.web/serverfarms" => new ResourceRetriever<ASP>(basicAzureResourceInfo, "2021-03-01"),
            "microsoft.web/sites" => new AppResourceRetriever(basicAzureResourceInfo),
            "microsoft.apimanagement/service" => new ApimServiceResourceRetriever(basicAzureResourceInfo),
            "microsoft.compute/virtualmachines" => new ResourceRetriever<VM>(basicAzureResourceInfo,
                "2021-07-01", true),
            "microsoft.compute/disks" => new ResourceRetriever<Disk>(basicAzureResourceInfo),
            "microsoft.operationalinsights/workspaces" => new ResourceRetriever<LogAnalyticsWorkspace>(
                basicAzureResourceInfo, fetchFullResource: true, apiVersion: "2021-06-01"),
            "microsoft.insights/components" => new ResourceRetriever<AppInsights>(basicAzureResourceInfo, "2020-02-02",
                true),
            "microsoft.storage/storageaccounts" => new ResourceRetriever<StorageAccount>(basicAzureResourceInfo,
                "2021-08-01", true),
            "microsoft.network/networksecuritygroups" => new ResourceRetriever<NSG>(basicAzureResourceInfo,
                fetchFullResource: true),
            "microsoft.network/publicipaddresses" => new ResourceRetriever<PIP>(basicAzureResourceInfo),
            "microsoft.compute/virtualmachines/extensions" =>
                new ResourceRetriever<VMExtension>(basicAzureResourceInfo, fetchFullResource: true,
                    apiVersion: "2021-11-01"),
            "microsoft.managedidentity/userassignedidentities" => new ResourceRetriever<UserAssignedManagedIdentity>(
                basicAzureResourceInfo),
            "microsoft.keyvault/vaults" => new ResourceRetriever<KeyVault>(basicAzureResourceInfo,
                "2019-09-01", true),
            "microsoft.insights/actiongroups" => new NoOpResourceRetriever(),
            "microsoft.alertsmanagement/smartdetectoralertrules" => new NoOpResourceRetriever(),
            "microsoft.compute/sshpublickeys" => new NoOpResourceRetriever(),
            "microsoft.insights/webtests" => new NoOpResourceRetriever(),
            "microsoft.sql/servers" => new ResourceRetriever<ManagedSqlServer>(basicAzureResourceInfo),
            "microsoft.sql/servers/databases" => new ResourceRetriever<ManagedSqlDatabase>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-08-01-preview"),
            "microsoft.web/kubeenvironments" => new ResourceRetriever<ContainerAppEnvironment>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-03-01"),
            "microsoft.web/containerapps" => new ResourceRetriever<ContainerApp>(basicAzureResourceInfo,
                fetchFullResource: true, apiVersion: "2021-03-01"),
            _ => new ResourceRetriever<AzureResource>(basicAzureResourceInfo)
        };
    }

    internal class AzureList<T>
    {
        public string NextLink { get; set; }
        public T[] Value { get; set; }
    }
}