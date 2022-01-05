using System;
using System.Collections.Generic;
using DrawIo.Azure.Core.Resources;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Diagrams;

internal class VNetDiagramResourceBuilder : IDiagramResourceBuilder
{
    private readonly VNet _resource;

    public VNetDiagramResourceBuilder(AzureResource resource)
    {
        _resource = (VNet)resource;
    }

    public IEnumerable<Node> CreateNodes()
    {
        var vnetNode = AzureResourceRectangleDrawer.CreateContainerRectangleNode(_resource.Name);
        yield return vnetNode;

        foreach (var subnet in _resource.Subnets)
        {
            var subnetNode = AzureResourceRectangleDrawer.CreateContainerRectangleNode(subnet.Name);
            vnetNode.AddChild(subnetNode);

            if (subnet.ContainedResources.Count == 0)
            {
                var emptyContents = AzureResourceRectangleDrawer.CreateSimpleRectangleNode(Guid.NewGuid().ToString());
                subnetNode.AddChild(emptyContents);
                yield return emptyContents;
            }

            yield return subnetNode;
        }
    }
}