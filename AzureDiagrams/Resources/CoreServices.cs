using System.Collections.Generic;
using System.Linq;

namespace AzureDiagrams.Resources;

public class CoreServices : AzureResource
{
    public override bool IsPureContainer => true;

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<KeyVault>().Where(x => x.Location == Location).ForEach(OwnsResource);
        allResources.OfType<StorageAccount>().Where(x => x.Location == Location).ForEach(OwnsResource);
        allResources.OfType<ACR>().Where(x => x.Location == Location).ForEach(OwnsResource);
        base.BuildRelationships(allResources);
    }
}