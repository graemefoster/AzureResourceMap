using AzureDiagrams.Resources;

namespace AzureDiagramsTests;

public static class AzResourceHelper
{
    public static Guid TestSubscriptionId = Guid.Parse("DE84C705-69EB-4EC1-8C35-CA648AC05E88");
    
    public static string GetResourceId(string resourceGroupName, string resourceName)
    {
        return $"/subscriptions/{TestSubscriptionId}/resourceGroups/{resourceGroupName}/{resourceName}";
    }

    public static void BuildRelationships(this AzureResource[] resources)
    {
        foreach (var resource in resources)
        {
            resource.BuildRelationships(resources);
        }
    }
}