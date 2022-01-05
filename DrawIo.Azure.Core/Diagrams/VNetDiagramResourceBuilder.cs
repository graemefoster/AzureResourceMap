using System.Collections.Generic;
using DrawIo.Azure.Core.Resources;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Diagrams;

internal class VNetDiagramResourceBuilder : AzureResourceNodeBuilder
{
    private readonly VNet _resource;

    public VNetDiagramResourceBuilder(AzureResource resource) : base(resource)
    {
        _resource = (VNet)resource;
    }

    protected override IEnumerable<(AzureResource, Node)> CreateNodesInternal(
        IDictionary<AzureResource, AzureResourceNodeBuilder> resourceNodeBuilders)
    {
        var vnetNode =
            AzureResourceRectangleDrawer.CreateContainerRectangleNode("VNet", _resource.Name, _resource.InternalId);
        yield return (_resource, vnetNode);

        foreach (var subnet in _resource.Subnets)
        {
            var subnetNode =
                AzureResourceRectangleDrawer.CreateContainerRectangleNode("Subnet", subnet.Name,
                    _resource.InternalId + $".{subnet.Name}");
            vnetNode.AddChild(subnetNode);

            if (subnet.ContainedResources.Count == 0)
            {
                var emptyContents = AzureResourceRectangleDrawer.CreateSimpleRectangleNode("Subnet", "Empty",
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