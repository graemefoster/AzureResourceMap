using System.Collections.Generic;
using System.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class PIP : AzureResource
{
    public override string Image => "img/lib/azure2/networking/Public_IP_Addresses.svg";

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<Nic>().Where(x => x.ExposedBy(this)).ForEach(nic => CreateFlowTo(nic, "From Public Internet"));
    }
}