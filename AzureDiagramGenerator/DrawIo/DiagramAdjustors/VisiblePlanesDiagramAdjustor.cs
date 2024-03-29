using AzureDiagrams.Resources;

namespace AzureDiagramGenerator.DrawIo.DiagramAdjustors;

public class VisiblePlanesDiagramAdjustor : IDiagramAdjustor
{
    private readonly Plane _visiblePlanes;

    public VisiblePlanesDiagramAdjustor(Plane visiblePlanes)
    {
        _visiblePlanes = visiblePlanes;
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
        return (link.Plane & _visiblePlanes) != Plane.None;
    }

    public void PostProcess(Dictionary<AzureResource, AzureResourceNodeBuilder> all)
    {
    }

    public AzureResource ReplacementFor(AzureResource resource)
    {
        return resource;
    }
}