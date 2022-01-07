using System.Collections.Generic;
using DrawIo.Azure.Core.Resources;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Diagrams;

internal class VNetDiagramResourceBuilder : AzureResourceNodeBuilder
{
    private readonly VNet _resource;

    public VNetDiagramResourceBuilder(VNet resource) : base(resource)
    {
        _resource = resource;
    }

    protected override IEnumerable<(AzureResource, Node)> CreateNodesInternal(
        IDictionary<AzureResource, AzureResourceNodeBuilder> resourceNodeBuilders)
    {
        var vnetNode =
            AzureResourceDrawer.CreateContainerRectangleNode("VNet", _resource.Name, _resource.InternalId, "#FFE6CC",
                TextAlignment.Top);
        yield return (_resource, vnetNode);


        if (_resource.PrivateDnsZones.Count > 0)
        {
            var privateDnsZoneCluster =
                AzureResourceDrawer.CreateContainerRectangleNode("", "DNS Zones",
                    _resource.InternalId + ".dnszones", "#E1D5E7", TextAlignment.Bottom);

            vnetNode.AddChild(privateDnsZoneCluster);
            foreach (var privateDnsZone in _resource.PrivateDnsZones)
            {
                var node = resourceNodeBuilders[privateDnsZone];
                foreach (var contained in CreateOtherResourceNodes(node, resourceNodeBuilders))
                {
                    if (contained.Item2.ClusterParent == null) privateDnsZoneCluster.AddChild(contained.Item2);
                    yield return contained;
                }
            }
        }

        foreach (var contained in _resource.ContainedResources)
        {
            var nodeBuilder = resourceNodeBuilders[contained];
            foreach (var containedNode in CreateOtherResourceNodes(nodeBuilder, resourceNodeBuilders))
            {
                if (containedNode.Item2.ClusterParent == null) vnetNode.AddChild(containedNode.Item2);
                yield return containedNode;
            }
        }

        foreach (var subnet in _resource.Subnets)
        {
            var subnetNode =
                AzureResourceDrawer.CreateContainerRectangleNode("Subnet", subnet.Name,
                    _resource.InternalId + $".{subnet.Name}", "white", TextAlignment.Bottom);

            vnetNode.AddChild(subnetNode);

            if (subnet.NSGs.Count > 0)
            {
                var nsgCluster =
                    AzureResourceDrawer.CreateContainerRectangleNode("", "NSGs",
                        _resource.InternalId + $".{subnet.Name}.nsgs", "#E1D5E7", TextAlignment.Bottom);

                subnetNode.AddChild(nsgCluster);
                foreach (var nsg in subnet.NSGs)
                {
                    var node = resourceNodeBuilders[nsg];
                    foreach (var contained in CreateOtherResourceNodes(node, resourceNodeBuilders))
                    {
                        if (contained.Item2.ClusterParent == null) nsgCluster.AddChild(contained.Item2);
                        yield return contained;
                    }
                }
            }

            if (subnet.ContainedResources.Count == 0)
            {
                var emptyContents = AzureResourceDrawer.CreateSimpleRectangleNode("Subnet", "Empty",
                    _resource.InternalId + $".{subnet.Name}.empty");
                subnetNode.AddChild(emptyContents);
                yield return (_resource, emptyContents);
            }
            else
            {
                foreach (var resource in subnet.ContainedResources)
                {
                    var node = resourceNodeBuilders[resource];
                    foreach (var contained in CreateOtherResourceNodes(node, resourceNodeBuilders))
                    {
                        if (contained.Item2.ClusterParent == null) subnetNode.AddChild(contained.Item2);

                        yield return contained;
                    }
                }
            }

            yield return (_resource, subnetNode);
        }
    }
}