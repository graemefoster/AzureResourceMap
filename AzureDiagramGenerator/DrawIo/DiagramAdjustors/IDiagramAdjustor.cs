using AzureDiagrams.Resources;

namespace AzureDiagramGenerator.DrawIo.DiagramAdjustors;

public interface IDiagramAdjustor
{
    string ImageFor(AzureResource resource);
    AzureResourceNodeBuilder? CreateNodeBuilder(AzureResource resource);
    bool DrawNode(AzureResource resource);
    bool DisplayLink(ResourceLink link);
    AzureResource? ReplacementFor(AzureResource resource);
}