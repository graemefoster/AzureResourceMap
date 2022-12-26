using AzureDiagrams.Resources;
using Newtonsoft.Json.Linq;

namespace AzureDiagramsTests;

public class TestResourcesObjectMother
{
    public static IEnumerable<AzureResource> WithPublicAccessibleStorageAccount()
    {
        yield return new StorageAccount()
        {
            Id = AzResourceHelper.GetResourceId(
                new Guid("9D7D2DC9-DFFA-4120-B896-0754D5D76486"),
                "test-rg",
                "storage123"),
            Name = "storage123",
            Type = "microsoft.storage/storageaccounts"
        };
    }

    public static async Task<IEnumerable<AzureResource>> VirtualNetwork(string subnet)
    {
        var vnet = new VNet()
        {
            Id = AzResourceHelper.GetResourceId(
                new Guid("266A0682-2A8F-4DBE-815A-D36956882FA3"),
                "test-rg",
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