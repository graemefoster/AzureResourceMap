using AzureDiagrams.Resources;

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
}