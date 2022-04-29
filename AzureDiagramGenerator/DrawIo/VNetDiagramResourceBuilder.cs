using System.Security.Cryptography;
using System.Text;
using AzureDiagramGenerator.DrawIo.DiagramAdjustors;
using AzureDiagrams.Resources;
using Microsoft.Msagl.Core.Layout;

namespace AzureDiagramGenerator.DrawIo;

internal class VNetDiagramResourceBuilder : AzureResourceNodeBuilder
{
    private readonly VNet _resource;

    public VNetDiagramResourceBuilder(VNet resource) : base(resource)
    {
        _resource = resource;
    }

    protected override IEnumerable<(AzureResource, Node)> CreateNodesInternal(
        IDictionary<AzureResource, AzureResourceNodeBuilder> resourceNodeBuilders,
        IDiagramAdjustor diagramAdjustor)
    {
        var vnetNode =
            AzureResourceDrawer.CreateContainerRectangleNode("VNet", _resource.Name, _resource.InternalId, "#F8CECC",
                TextAlignment.Top, _resource.Image);
        
        yield return (_resource, vnetNode);

        if (_resource.PrivateDnsZones.Count > 0)
        {
            var privateDnsZoneCluster =
                AzureResourceDrawer.CreateContainerRectangleNode("", "DNS Zones",
                    _resource.InternalId + ".dnszones", "#E1D5E7", TextAlignment.Bottom);

            var dnsZoneImage = AzureResourceDrawer.CreateSimpleImageNode(_resource.PrivateDnsZones[0].Image, "Private Dns", _resource.Id + "_dns");
            vnetNode.AddChild(privateDnsZoneCluster);
            privateDnsZoneCluster.AddChild(dnsZoneImage);

            var displayText = string.Join("&#xa;", _resource.PrivateDnsZones.Select(x => x.Name));
            var id = new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(displayText + _resource.InternalId))[..16]).ToString();
            var zoneText = AzureResourceDrawer.CreateTextNode(displayText, id);
            privateDnsZoneCluster.AddChild(zoneText);

            yield return (_resource, dnsZoneImage);
            yield return (_resource, zoneText);
        }

        foreach (var contained in _resource.ContainedResources)
        {
            var nodeBuilder = resourceNodeBuilders[contained];
            foreach (var containedNode in CreateOtherResourceNodes(nodeBuilder, resourceNodeBuilders, diagramAdjustor))
            {
                if (containedNode.Item2.ClusterParent == null) vnetNode.AddChild(containedNode.Item2);
                yield return containedNode;
            }
        }

        foreach (var subnet in _resource.Subnets)
        {
            var images = new List<string>();
            if (subnet.NSGs.Any()) images.Add("img/lib/azure2/networking/Network_Security_Groups.svg");
            if (subnet.UdrId != null) images.Add("img/lib/azure2/networking/Route_Tables.svg");
            
            var subnetNode =
                AzureResourceDrawer.CreateContainerRectangleNode(subnet.AddressPrefix, subnet.Name,
                    _resource.InternalId + $".{subnet.Name}", "white", TextAlignment.Top,
                    images.ToArray());

            vnetNode.AddChild(subnetNode);

            if (subnet.ContainedResources.Count == 0)
            {
                var emptyContents = AzureResourceDrawer.CreateSimpleRectangleNode("Subnet", "Empty",
                    _resource.InternalId + $".{subnet.Name}.empty", backgroundColour:"#ffffff");
                subnetNode.AddChild(emptyContents);
                yield return (_resource, emptyContents);
            }
            else
            {
                foreach (var resource in subnet.ContainedResources)
                {
                    var node = resourceNodeBuilders[resource];
                    foreach (var contained in CreateOtherResourceNodes(node, resourceNodeBuilders, diagramAdjustor))
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