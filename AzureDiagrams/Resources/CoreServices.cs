using System.Collections.Generic;
using System.Linq;

namespace DrawIo.Azure.Core.Resources;

public class CoreServices : AzureResource
{
    public override bool IsPureContainer => true;

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<KeyVault>().ForEach(OwnsResource);
        allResources.OfType<StorageAccount>().ForEach(OwnsResource);
        base.BuildRelationships(allResources);
    }
}