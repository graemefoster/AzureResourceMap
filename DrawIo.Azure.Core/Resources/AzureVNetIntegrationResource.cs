using System.Collections.Generic;
using System.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class AzureVNetIntegrationResource : AzureResource
{
    private readonly string _vnetIntegratedInto;

    public AzureVNetIntegrationResource(string id, string vnetIntegratedInto)
    {
        Id = id;
        _vnetIntegratedInto = vnetIntegratedInto;
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var segments = _vnetIntegratedInto.Split('/');
        var vnetInfo = new { vnet = segments.ElementAt(segments.Length - 3), subnet = segments.Last() };

        var vNet = allResources.OfType<VNet>().Single(x => x.Name == vnetInfo.vnet);
        vNet.InjectResourceInto(this, vnetInfo.subnet);
        this.ContainedByAnotherResource = true;

    }
}