using System.Collections.Generic;
using System.Linq;

namespace AzureDiagrams.Resources;

public class PublicIpAddresses : AzureResource
{
    public override bool IsPureContainer => true;

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<PIP>().Where(x => x.Location == Location).ForEach(OwnsResource);
        base.BuildRelationships(allResources);
    }
}