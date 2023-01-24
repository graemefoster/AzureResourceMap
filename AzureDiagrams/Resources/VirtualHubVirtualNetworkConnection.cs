using System.Collections.Generic;

namespace AzureDiagrams.Resources;

public class VirtualHubVirtualNetworkConnection : AzureResource
{
    public override string Image => "img/lib/azure2/networking/Virtual_WANs.svg";
    public VHub VHub { get; init; }
    public VNet LinkedVNet { get; init; }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        CreateFlowTo(VHub, Plane.All);
    }
}