using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureDiagrams.Resources;

public class VWan : AzureResource
{
    public override string Image => "img/lib/mscae/Virtual_WANs.svg";
    public override string? Fill => "#dae8fc";

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<VHub>().Where(x => x.VWanId.Equals(Id, StringComparison.InvariantCultureIgnoreCase)).ForEach(OwnsResource);
        base.BuildRelationships(allResources);
    }
   
}