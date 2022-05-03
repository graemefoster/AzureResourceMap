using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureDiagrams.Resources;

internal class PIP : AzureResource
{
    public override string Image => "img/lib/azure2/networking/Public_IP_Addresses.svg";

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<ICanExposePublicIPAddresses>()
            .Where(x => x.PublicIpAddresses.Any(x =>
                string.Compare(Id, x, StringComparison.InvariantCultureIgnoreCase) == 0))
            .ForEach(x => CreateFlowTo((AzureResource)x, "From Public Internet", Plane.Runtime));
        base.BuildRelationships(allResources);
    }
}