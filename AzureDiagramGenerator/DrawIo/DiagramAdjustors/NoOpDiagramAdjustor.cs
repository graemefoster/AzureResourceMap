using AzureDiagrams.Resources;

namespace AzureDiagramGenerator.DrawIo.DiagramAdjustors;

public class NoOpDiagramAdjustor : IDiagramAdjustor
{
    private readonly bool _noInfer;

    public NoOpDiagramAdjustor(bool noInfer)
    {
        _noInfer = noInfer;
    }

    public string ImageFor(AzureResource resource)
    {
        return resource.Image;
    }

    public AzureResourceNodeBuilder? CreateNodeBuilder(AzureResource resource)
    {
        return null;
    }

    public bool DisplayLink(ResourceLink link)
    {
        return !_noInfer || link.FlowEmphasis != FlowEmphasis.Inferred;
    }
    
    public AzureResource ReplacementFor(AzureResource resource)
    {
        return resource;
    }
}