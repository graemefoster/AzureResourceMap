using System.Collections.Generic;
using DrawIo.Azure.Core.Resources;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Diagrams;

internal class AzureResourceNodeBuilder : IDiagramResourceBuilder
{
    private readonly AzureResource _resource;

    public AzureResourceNodeBuilder(AzureResource resource)
    {
        _resource = resource;
    }

    public IEnumerable<Node> CreateNodes()
    {
        yield return AzureResourceRectangleDrawer.CreateSimpleRectangleNode(_resource.Name);
    }
}