using AzureDiagrams.Resources;

namespace AzureDiagramsTests;

public static class AzResourceHelper
{
    public static Guid TestSubscriptionId = Guid.Parse("DE84C705-69EB-4EC1-8C35-CA648AC05E88");

    public static string GetResourceId(string resourceGroupName, string resourceName)
    {
        return $"/subscriptions/{TestSubscriptionId}/resourceGroups/{resourceGroupName}/{resourceName}";
    }

    public static AzureResource[] Process(this AzureResource[] resources)
    {
        var initialResources = resources.ToList();
        var allResources = initialResources.Concat(initialResources.SelectMany(x => x.DiscoverNewNodes(initialResources))).ToArray();
        foreach (var resource in allResources)
        {
            resource.BuildRelationships(allResources);
        }
        return allResources;
    }
}