using AzureDiagrams.Resources;
using AzureDiagrams.Resources.Retrievers.Extensions;
using Newtonsoft.Json.Linq;

namespace AzureDiagramsTests;

public class TestResourcesObjectMother
{
    public static IEnumerable<AzureResource> StorageAccount()
    {
        yield return new StorageAccount()
        {
            Id = AzResourceHelper.GetResourceId("test-rg",
                "storage123"),
            Name = "storage123",
            Type = "microsoft.storage/storageaccounts"
        };
    }

    public static async Task<IEnumerable<AzureResource>> StorageAccountWithPrivateEndpoint()
    {
        var vnet = (VNet)(await VirtualNetwork("test-subnet")).Single();

        var storage = new StorageAccount()
        {
            Id = AzResourceHelper.GetResourceId("test-rg",
                "storage123"),
            Name = "storage123",
            Type = "microsoft.storage/storageaccounts",
            Extensions = new[] { new PrivateEndpointExtensions() }
        };
        var peId = new Guid("A1621A82-22D1-495D-8B9F-AF87F31D21C2");

        var privateEndpointResourceId = AzResourceHelper.GetResourceId("test-rg",
            $"pe-{peId}");

        var rawStorageJson = JObject.FromObject(new
        {
            properties = new
            {
                privateEndpointConnections = new[]
                {
                    new
                    {
                        properties = new
                        {
                            privateEndpoint = new
                            {
                                id = privateEndpointResourceId
                            }
                        }
                    }
                }
            }
        });
        await storage.Enrich(rawStorageJson, new Dictionary<string, JObject?>());
        storage.Extensions.ForEach(x => x.Enrich(storage, rawStorageJson, new Dictionary<string, JObject?>()));

        var nicId = new Guid("BD4F785D-6E10-40C5-9BF1-D04B20ECA9BF");

        var nic = new Nic()
        {
            Id = AzResourceHelper.GetResourceId("test-rg",
                $"pe-nic-{nicId}"),
        };

        await nic.Enrich(JObject.FromObject(new
        {
            properties = new
            {
                ipConfigurations = new[]
                {
                    new
                    {
                        properties = new
                        {
                            subnet = new
                            {
                                id = $"{vnet.Id}/subnets/{vnet.Subnets[0].Name}"
                            }
                        }
                    }
                }
            }
        }), new Dictionary<string, JObject?>());

        var pe = new PrivateEndpoint()
        {
            Id = privateEndpointResourceId,
        };

        await pe.Enrich(JObject.FromObject(new
        {
            properties = new
            {
                networkInterfaces = new[]
                {
                    new
                    {
                        id = nic.Id
                    }
                },
                customDnsConfigs = new[]
                {
                    new
                    {
                        fqdn = "test-pe.localtest.me"
                    }
                },
                subnet = new
                {
                    id = $"{vnet.Id}/subnets/{vnet.Subnets[0].Name}"
                }
            }
        }), new Dictionary<string, JObject?>());

        var allResources = new AzureResource[] { vnet, storage, pe, nic };
        allResources.BuildRelationships();
        return allResources;
    }

    public static async Task<IEnumerable<AzureResource>> VirtualNetwork(string subnet)
    {
        var vnet = new VNet()
        {
            Id = AzResourceHelper.GetResourceId("test-rg",
                "vnet123"),
        };
        await vnet.Enrich(JObject.FromObject(new
        {
            properties = new
            {
                subnets = new[]
                {
                    new
                    {
                        name = subnet,
                        properties = new
                        {
                            addressPrefix = "10.0.0.0/24"
                        }
                    }
                }
            }
        }), new Dictionary<string, JObject?>());
        return new[] { vnet };
    }
}