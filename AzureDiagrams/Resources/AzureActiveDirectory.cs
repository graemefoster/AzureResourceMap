using System.Collections.Generic;
using System.Linq;

namespace AzureDiagrams.Resources;

public class AzureActiveDirectory : AzureResource
{
    public override bool IsPureContainer => true;

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<UserAssignedManagedIdentity>().ForEach(OwnsResource);
        base.BuildRelationships(allResources);
    }
}