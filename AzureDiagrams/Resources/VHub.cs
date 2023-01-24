using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using AzureDiagrams.Resources.Retrievers.Custom;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class VHub : AzureResource
{
    public override string Fill => "#d5e8d4";

    public VHub(string vWanId, string id, string name, string[] connectedVirtualNetworkIds)
    {
        Id = id;
        Name = name;
        VWanId = vWanId;
        ConnectedVirtualNetworkIds = connectedVirtualNetworkIds;
    }

    [JsonConstructor]
    public VHub()
    {
    }

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

    public override IEnumerable<AzureResource> DiscoverNewNodes(List<AzureResource> azureResources)
    {
        foreach (var vnet in ConnectedVirtualNetworkIds)
        {
            var vnetResource = azureResources.OfType<VNet>()
                .SingleOrDefault(x => x.Id.Equals(vnet, StringComparison.InvariantCultureIgnoreCase));
            if (vnetResource != null)
            {
                var vnetConnection = new VirtualHubVirtualNetworkConnection()
                {
                    VHub = this,
                    Id = $"{Id}.{vnet}",
                    LinkedVNet = vnetResource,
                    Name = "Virtual Hub Link"
                };
                vnetResource.LinksToVHub(vnetConnection);
                yield return vnetConnection;
            }
        }
    }

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

        //Issue with the SugiyamaLayoutSettings drawing an edge from a cluster (which is how the VNet appears) to a node... Instead we will need to gen a new node to represent the VHub -> VNet connection, and draw the link between them 


        base.BuildRelationships(allResources);
    }
}