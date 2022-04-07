using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class DnsZoneVirtualNetworkLink : AzureResource
{
    private string _dnsZone = default!;
    private string _virtualNetwork = default!;

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        _virtualNetwork = full["properties"]!["virtualNetwork"]!.Value<string>("id")!;
        _dnsZone = string.Join('/', Id.Split("/").ToArray()[..^2]);
        return Task.CompletedTask;
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var dnsZone = allResources.OfType<PrivateDnsZone>().Single(x => x.Id == _dnsZone);
        var vnet = allResources.OfType<VNet>().SingleOrDefault(x => x.Id == _virtualNetwork);
        if (vnet != null)
        {
            vnet?.AssignPrivateDnsZone(dnsZone);
            dnsZone.ContainedByAnotherResource = true;
        }
        else
        {
            Console.WriteLine($"Failed to find VNET {_virtualNetwork} to link to DNS Zone {_dnsZone}");
        }

        base.BuildRelationships(allResources);
    }
}