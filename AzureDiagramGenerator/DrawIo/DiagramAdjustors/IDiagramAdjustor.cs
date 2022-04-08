using AzureDiagrams.Resources;

namespace AzureDiagramGenerator.DrawIo.DiagramAdjustors;

public interface IDiagramAdjustor
{
    string ImageFor(AzureResource resource);
    AzureResourceNodeBuilder? CreateNodeBuilder(AzureResource resource);
    bool DisplayLink(ResourceLink link);

    /// <summary>
    /// Either return the original resource, or a replacement if you want to reroute this nodes links via somewhere else.
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    AzureResource ReplacementFor(AzureResource resource);
}