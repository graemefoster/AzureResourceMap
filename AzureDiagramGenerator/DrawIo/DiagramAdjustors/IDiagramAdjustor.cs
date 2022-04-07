using AzureDiagrams.Resources;

namespace AzureDiagramGenerator.DrawIo.DiagramAdjustors;

public interface IDiagramAdjustor
{
    string ImageFor(AzureResource resource);
    AzureResourceNodeBuilder? CreateNodeBuilder(AzureResource resource);
    bool DrawNode(AzureResource resource);
    AzureResource? ReplacementFor(AzureResource resource);
}