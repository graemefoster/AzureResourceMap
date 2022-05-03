using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureDiagrams.Resources.Retrievers.Custom;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class VHub : AzureResource
{
    public override string Image => "img/lib/azure2/networking/Virtual_WANs.svg";
    public override string Fill => "#d5e8d4";

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        VWanId = full["properties"]!["virtualWan"]!.Value<string>("id")!;
        ConnectedVirtualNetworkIds = additionalResources[VHubRetriever.VirtualNetworkConnections]!["value"]?
                                         .Select(x => x["properties"]!["remoteVirtualNetwork"]!.Value<string>("id"))
                                         .Select(x => x!).ToArray() ??
                                     Array.Empty<string>();
        FirewallId = full["properties"]!["azureFirewall"]?.Value<string>("id");

        return base.Enrich(full, additionalResources);
    }

    public string? FirewallId { get; set; }

    public string[] ConnectedVirtualNetworkIds { get; set; } = default!;

    public string VWanId { get; set; } = default!;

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        if (FirewallId != null)
        {
            var firewall = allResources.OfType<Firewall>()
                .SingleOrDefault(x => x.Id.Equals(FirewallId, StringComparison.InvariantCultureIgnoreCase));
            if (firewall != null)
            {
                OwnsResource(firewall);
            }
        }

        allResources.OfType<P2S>()
            .Where(x => x.VHubId?.Equals(Id, StringComparison.InvariantCultureIgnoreCase) ?? false)
            .ForEach(OwnsResource);
        allResources.OfType<S2S>()
            .Where(x => x.VHubId?.Equals(Id, StringComparison.InvariantCultureIgnoreCase) ?? false)
            .ForEach(OwnsResource);

        ConnectedVirtualNetworkIds.ForEach(x =>
            allResources.SingleOrDefault(r => r.Id.Equals(x, StringComparison.InvariantCultureIgnoreCase))
                ?.CreateFlowTo(this, Plane.All));
        base.BuildRelationships(allResources);
    }
}