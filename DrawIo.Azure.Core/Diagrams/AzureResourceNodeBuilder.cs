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
                var from = nodes[_resource].Single(x => ((CustomUserData)x.UserData).Id == link.From.InternalId);
                var to = nodes[link.To].Single(x => ((CustomUserData)x.UserData).Id == link.To.InternalId);
                yield return AzureResourceDrawer.CreateSimpleEdge(from, to, link.Details, link.FlowEmphasis);
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
        Cluster? container = null;

        if (_resource.ContainedResources.Count > 0)
        {
            container = AzureResourceDrawer.CreateContainerRectangleNode(_resource.Type, _resource.Name,
                $"{_resource.InternalId}.container", "#FFE6CC", TextAlignment.Top);
            yield return (_resource, container);
            foreach (var contained in _resource.ContainedResources)
            {
                var nodeBuilder = resourceNodeBuilders[contained];
                foreach (var containedNode in CreateOtherResourceNodes(nodeBuilder, resourceNodeBuilders))
                {
                    if (containedNode.Item2.ClusterParent == null) container.AddChild(containedNode.Item2);
                    yield return containedNode;
                }
            }
        }

        if (!_resource.IsPureContainer)
        {
            if (string.IsNullOrEmpty(_resource.Image))
            {
                var resourceNode =
                    AzureResourceDrawer.CreateSimpleRectangleNode(_resource.GetType().Name, _resource.Name,
                        _resource.InternalId);
                if (container != null) container.AddChild(resourceNode);
                yield return (_resource, resourceNode);
            }
            else
            {
                var resourceNode =
                    AzureResourceDrawer.CreateSimpleImageNode(_resource.Image, _resource.Name, _resource.InternalId);
                if (container != null) container.AddChild(resourceNode);
                yield return (_resource, resourceNode);
            }
        }
    }
}