using System.Collections.Generic;
using DrawIo.Azure.Core.Resources;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Diagrams;

internal class AppServicePlanDiagramResourceBuilder : AzureResourceNodeBuilder
{
    private readonly ASP _resource;

    public AppServicePlanDiagramResourceBuilder(ASP resource) : base(resource)
    {
        _resource = resource;
    }

    protected override IEnumerable<(AzureResource, Node)> CreateNodesInternal(
        IDictionary<AzureResource, AzureResourceNodeBuilder> resourceNodeBuilders)
    {
        var appServicePlanNode =
            AzureResourceDrawer.CreateContainerRectangleNode("ASP", _resource.Name, _resource.InternalId,
                backgroundColour: "#D5E8D4");

        yield return (_resource, appServicePlanNode);

        foreach (var containedApp in _resource.ContainedApps)
        {
            var node = resourceNodeBuilders[containedApp];
            foreach (var app in CreateOtherResourceNodes(node, resourceNodeBuilders))
            {
                appServicePlanNode.AddChild(app.Item2);
                yield return app;
            }
        }
    }
}