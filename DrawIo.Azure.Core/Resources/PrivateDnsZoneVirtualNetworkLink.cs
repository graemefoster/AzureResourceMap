using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Diagrams;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class PrivateDnsZoneVirtualNetworkLink : AzureResource
{
    private string _dnsZone;
    private string _virtualNetwork;

    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new IgnoreNodeBuilder(this);
    }

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        _virtualNetwork = full["properties"]!["virtualNetwork"]!.Value<string>("id")!;
        _dnsZone = string.Join('/', Id.Split("/").ToArray()[..^2]);
        return Task.CompletedTask;
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var dnsZone = allResources.OfType<PrivateDnsZone>().Single(x => x.Id == _dnsZone);
        allResources.OfType<VNet>().Single(x => x.Id == _virtualNetwork).AssignPrivateDnsZone(dnsZone);
        dnsZone.ContainedByAnotherResource = true;
    }
}