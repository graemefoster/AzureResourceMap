using AzureDiagrams.Resources;

namespace AzureDiagramGenerator.DrawIo.DiagramAdjustors;

public class RawDiagramAdjustor : IDiagramAdjustor
{
    public string ImageFor(AzureResource resource)
    {
        return resource.Image;
    }

    public AzureResourceNodeBuilder? CreateNodeBuilder(AzureResource resource)
    {
        return null;
    }

    public bool DrawNode(AzureResource resource)
    {
        return true;
    }
    
    public AzureResource? ReplacementFor(AzureResource resource)
    {
        return null;
    }
}