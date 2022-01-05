using System;
using System.Collections.Generic;
using System.Linq;
using DrawIo.Azure.Core.Resources;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Diagrams;

public class AzureResourceNodeBuilder
{
    private readonly AzureResource _resource;

    public AzureResourceNodeBuilder(AzureResource resource)
    {
        _resource = resource;
    }

    public IEnumerable<(AzureResource, Node)> CreateNodes(
        IDictionary<AzureResource, AzureResourceNodeBuilder> resourceNodeBuilders)
    {
        if (_resource.ContainedByAnotherResource) yield break;
        foreach (var node in CreateNodesInternal(resourceNodeBuilders)) yield return node;
    }

    public IEnumerable<Edge> CreateEdges(IDictionary<AzureResource, Node[]> nodes)
    {
        foreach (var link in _resource.Links)
            if (!(nodes.ContainsKey(link.To) && nodes.ContainsKey(_resource)))
            {
                Console.WriteLine("Ignoring edge as not all nodes represented");
            }
            else
            {
                var from = nodes[_resource].Single();
                var to = nodes[link.To].Single();
                yield return AzureResourceDrawer.CreateSimpleEdge(from, to);
            }
    }

    protected IEnumerable<(AzureResource, Node)> CreateOtherResourceNodes(AzureResourceNodeBuilder otherResource,
        IDictionary<AzureResource, AzureResourceNodeBuilder> resourceNodeBuilders)
    {
        foreach (var node in otherResource.CreateNodesInternal(resourceNodeBuilders)) yield return node;
    }

    protected virtual IEnumerable<(AzureResource, Node)> CreateNodesInternal(
        IDictionary<AzureResource, AzureResourceNodeBuilder> resourceNodeBuilders)
    {
        if (string.IsNullOrEmpty(_resource.Image))
        {
            yield return (_resource,
                AzureResourceDrawer.CreateSimpleRectangleNode(_resource.GetType().Name, _resource.Name,
                    _resource.InternalId));
        } else
        {
            yield return (_resource,
                AzureResourceDrawer.CreateSimpleImageNode(_resource.Image, _resource.Name, _resource.InternalId));
        }
    }
}