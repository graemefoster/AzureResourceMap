using AzureDiagramGenerator.DrawIo.DiagramAdjustors;
using AzureDiagrams.Resources;
using Microsoft.Msagl.Core.Layout;

namespace AzureDiagramGenerator.DrawIo;

internal class IgnoreNodeBuilder : AzureResourceNodeBuilder
{
    public IgnoreNodeBuilder(AzureResource resource) : base(resource)
    {
    }

    protected override IEnumerable<(AzureResource, Node)> CreateNodesInternal(
        IDictionary<AzureResource, AzureResourceNodeBuilder> resourceNodeBuilders, IDiagramAdjustor diagramAdjustor)
    {
        yield break;
    }
}