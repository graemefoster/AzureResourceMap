using AzureDiagramGenerator.DrawIo.DiagramAdjustors;
using AzureDiagrams.Resources;
using Microsoft.Msagl.Core.Layout;

namespace AzureDiagramGenerator.DrawIo;

public class AzureResourceNodeBuilder
{
    private readonly AzureResource _resource;

    public AzureResourceNodeBuilder(AzureResource resource)
    {
        _resource = resource;
    }

    public IEnumerable<(AzureResource, Node)> CreateNodes(
        IDictionary<AzureResource, AzureResourceNodeBuilder> resourceNodeBuilders,
        IDiagramAdjustor diagramAdjustor)
    {
        if (_resource.ContainedByAnotherResource) yield break;
        foreach (var node in CreateNodesInternal(resourceNodeBuilders, diagramAdjustor)) yield return node;
    }

    public IEnumerable<Edge> CreateEdges(
        IDictionary<AzureResource, Node[]> nodes,
        IDiagramAdjustor diagramAdjustor)
    {
        foreach (var link in _resource.Links)
        {
            var isLinkVisible = diagramAdjustor.DisplayLink(link);

            var fromResource = diagramAdjustor.ReplacementFor(link.From);
            var toResource = diagramAdjustor.ReplacementFor(link.To);

            if (fromResource == toResource)
            {
                continue;
            }

            //We switched a resource. Don't double up the links 
            if (fromResource != link.From || toResource != link.To)
            {
                //check the replaced node doesn't already contain a link to this target. If it does we will show multiple links between 2 resources.
                if (fromResource.Links.Any(x => x.To == toResource)) continue;
            }

            if (!(nodes.ContainsKey(toResource) && nodes.ContainsKey(fromResource)))
            {
                Console.WriteLine(
                    $"Ignoring edge from {fromResource.Name} -> {toResource.Name} as not all nodes represented");
            }
            else
            {
                var from = nodes[fromResource].Single(x => ((CustomUserData)x.UserData).Id == fromResource.InternalId);
                var to = nodes[toResource].Single(x => ((CustomUserData)x.UserData).Id == toResource.InternalId);
                yield return AzureResourceDrawer.CreateSimpleEdge(link.From, link.To, from, to, link.Details,
                    link.Plane, isLinkVisible);
            }
        }
    }

    protected IEnumerable<(AzureResource, Node)> CreateOtherResourceNodes(
        AzureResourceNodeBuilder otherResource,
        IDictionary<AzureResource, AzureResourceNodeBuilder> resourceNodeBuilders,
        IDiagramAdjustor diagramAdjustor)
    {
        foreach (var node in otherResource.CreateNodesInternal(resourceNodeBuilders, diagramAdjustor))
            yield return node;
    }

    protected virtual IEnumerable<(AzureResource, Node)> CreateNodesInternal(
        IDictionary<AzureResource, AzureResourceNodeBuilder> resourceNodeBuilders,
        IDiagramAdjustor diagramAdjustor)
    {
        Cluster? container = null;

        if (_resource.ContainedResources.Count > 0)
        {
            container = AzureResourceDrawer.CreateContainerRectangleNode(_resource.Type ?? _resource.GetType().Name,
                _resource.Name,
                $"{_resource.InternalId}.container", _resource.Fill, TextAlignment.Top);
            yield return (_resource, container);
            foreach (var contained in _resource.ContainedResources)
            {
                var nodeBuilder = resourceNodeBuilders[contained];
                foreach (var containedNode in CreateOtherResourceNodes(nodeBuilder, resourceNodeBuilders,
                             diagramAdjustor))
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
                        _resource.InternalId, backgroundColour: _resource.Fill);
                if (container != null) container.AddChild(resourceNode);
                yield return (_resource, resourceNode);
            }
            else
            {
                var resourceNode = AzureResourceDrawer.CreateSimpleImageNode(diagramAdjustor.ImageFor(_resource),
                    _resource.Name,
                    _resource.InternalId);
                if (container != null) container.AddChild(resourceNode);
                yield return (_resource, resourceNode);
            }
        }
    }

    public static AzureResourceNodeBuilder CreateNodeBuilder(AzureResource resource, IDiagramAdjustor diagramAdjustor)
    {
        var special = diagramAdjustor.CreateNodeBuilder(resource);
        if (special != null) return special;

        return resource.GetType() switch
        {
            _ when resource is DnsZoneVirtualNetworkLink => new IgnoreNodeBuilder(resource),
            _ when resource is VNet vnet => new VNetDiagramResourceBuilder(vnet),
            _ => GetResourceBuilder(resource)
        };
    }

    private static AzureResourceNodeBuilder GetResourceBuilder(AzureResource resource)
    {
        return resource.Type?.ToLowerInvariant() switch
        {
            null => new AzureResourceNodeBuilder(resource),
            "microsoft.compute/virtualmachines/extensions" => new IgnoreNodeBuilder(resource),
            "microsoft.alertsmanagement/smartdetectoralertrules" => new IgnoreNodeBuilder(resource),
            "microsoft.compute/sshpublickeys" => new IgnoreNodeBuilder(resource),
            "microsoft.insights/webtests" => new IgnoreNodeBuilder(resource),
            "microsoft.insights/actiongroups" => new IgnoreNodeBuilder(resource),
            "microsoft.operationsmanagement/solutions" => new IgnoreNodeBuilder(resource),
            "microsoft.network/firewallpolicies" => new IgnoreNodeBuilder(resource),
            "microsoft.security/iotsecuritysolutions" => new IgnoreNodeBuilder(resource),
            "microsoft.insights/autoscalesettings" => new IgnoreNodeBuilder(resource),
            "microsoft.network/dnszones" => new IgnoreNodeBuilder(resource),
            "microsoft.customproviders/resourceproviders" => new IgnoreNodeBuilder(resource),
            "microsoft.web/certificates" => new IgnoreNodeBuilder(resource),
            "microsoft.network/vpnserverconfigurations" => new IgnoreNodeBuilder(resource),
            "microsoft.network/privatednszones" => new IgnoreNodeBuilder(resource),
            "microsoft.network/networkprofiles" => new IgnoreNodeBuilder(resource),
            "microsoft.resources/deploymentscripts" => new IgnoreNodeBuilder(resource),
            "microsoft.insights/datacollectionendpoints" => new IgnoreNodeBuilder(resource),
            "microsoft.insights/datacollectionrules" => new IgnoreNodeBuilder(resource),
            "microsoft.network/networksecuritygroups" => new IgnoreNodeBuilder(resource),
            "microsoft.network/routetables" => new IgnoreNodeBuilder(resource),
            "microsoft.portal/dashboards" => new IgnoreNodeBuilder(resource),
            _ => new AzureResourceNodeBuilder(resource)
        };
    }
}