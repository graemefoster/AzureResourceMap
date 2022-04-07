using System.Collections.Immutable;
using AzureDiagrams.Resources;

namespace AzureDiagramGenerator.DrawIo.DiagramAdjustors;

public class CondensedDiagramAdjustor : IDiagramAdjustor
{
    private readonly bool _noInfer;
    private readonly Dictionary<AzureResource, AzureResource> _replacements;

    public CondensedDiagramAdjustor(AzureResource[] allResources, bool noInfer)
    {
        _noInfer = noInfer;
        //Get rid of Private Endpoints where there is a NIC attached.
        var replacements = allResources.OfType<Nic>().Where(x => x.ConnectedPrivateEndpoint != null)
            .Select(x => (remove: (AzureResource)x.ConnectedPrivateEndpoint!, replaceWith: (AzureResource)x))
            .ToDictionary(x => x.remove, x => x.replaceWith);

        //Get rid of any resource where there's a private endpoint involved. We will collapse into the VNet
        //Careful - multiple private endpoints can point to the same resource. This is always going to have the potential to be leaky.
        var peReplacements = allResources.OfType<PrivateEndpoint>()
            .Where(x => x.ResourceAccessedByMe != null)
            .Where(x => replacements.ContainsKey(x))
            .Select(x => (remove: x.ResourceAccessedByMe!, replaceWith: replacements[x]));
        
        var vnetIntReplacements = allResources.OfType<App>()
            .Where(x => peReplacements.Any(y => y.remove == x))
            .Where(x => x.VNetIntegration != null)
            .Select(x => (remove: x.VNetIntegration!, replaceWith: peReplacements.First(y => y.remove == x).replaceWith));

        foreach (var replacement in peReplacements)
        {
            if (!replacements.ContainsKey(replacement.remove)) replacements.Add(replacement.remove, replacement.replaceWith);
        }

        foreach (var replacement in vnetIntReplacements)
        {
            if (!replacements.ContainsKey(replacement.remove)) replacements.Add(replacement.remove, replacement.replaceWith);
        }
        
        _replacements = replacements;
    }

    public string ImageFor(AzureResource resource)
    {
        var real = resource;
        while (_replacements.ContainsKey(real))
        {
            real = _replacements[real];
        }

        return real.Image;
    }

    public AzureResourceNodeBuilder? CreateNodeBuilder(AzureResource resource)
    {
        if (_replacements.ContainsKey(resource)) return new IgnoreNodeBuilder(resource);
        return null;
    }

    public bool DrawNode(AzureResource resource)
    {
        return !_replacements.ContainsKey(resource);
    }

    public AzureResource? ReplacementFor(AzureResource resource)
    {
        if (_replacements.TryGetValue(resource, out var res))
        {
            return res;
        }
        return null;
    }
    
    public bool DisplayLink(ResourceLink link)
    {
        return !_noInfer || link.FlowEmphasis != FlowEmphasis.Inferred;
    }

}