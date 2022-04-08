using AzureDiagrams.Resources;

namespace AzureDiagramGenerator.DrawIo.DiagramAdjustors;

public class CondensedDiagramAdjustor : IDiagramAdjustor
{
    private readonly bool _noInfer;
    private readonly Dictionary<AzureResource, AzureResource> _replacements = new();
    private readonly List<AzureResource> _removals = new();

    public CondensedDiagramAdjustor(AzureResource[] allResources, bool noInfer)
    {
        _noInfer = noInfer;

        CollapsePrivateEndpoints(allResources);
        _removals.AddRange(_replacements.Keys);
    }

    private void CollapsePrivateEndpoints(AzureResource[] allResources)
    {
        var replacements = allResources.OfType<PrivateEndpoint>()
            .Where(x => x.ResourceAccessedByMe != null)
            .Select(x => (pe: x, res: x.ResourceAccessedByMe!))
            .Select(x => (x.res, x.pe))
            .GroupBy(x => x.res,
                e => (pe: e.pe, nic: allResources.OfType<Nic>().Single(nic => nic.ConnectedPrivateEndpoint == e.pe)))
            .ToArray();

        foreach (var grouping in replacements)
        {
            var distinctSubnets = grouping.SelectMany(x => x.pe.SubnetIdsIAmInjectedInto).Distinct();
            if (distinctSubnets.Count() == 1)
            {
                //create mappings:
                var currentPe = grouping.First();
                _replacements.Add(grouping.Key, currentPe.pe);
                foreach (var secondaryPe in grouping.Skip(1))
                {
                    _replacements.Add(currentPe.pe, secondaryPe.pe);
                    currentPe = secondaryPe;
                }

                _replacements.Add(currentPe.pe, currentPe.nic);

                //and reverse through the nics
                foreach (var secondaryPe in grouping.Reverse().Skip(1))
                {
                    _replacements.Add(currentPe.nic, secondaryPe.nic);
                    currentPe = secondaryPe;
                }

                //finally target vnet integration
                if (grouping.Key is App { VNetIntegration: { } } app)
                {
                    _replacements.Add(app.VNetIntegration!, currentPe.nic);
                }
            }
        }
    }

    public string ImageFor(AzureResource resource)
    {
        return resource switch
        {
            Nic { ConnectedPrivateEndpoint: not null } res =>
                res.ConnectedPrivateEndpoint!.ResourceAccessedByMe?.Image ?? res.Image,
            _ => resource.Image
        };
    }

    public AzureResourceNodeBuilder? CreateNodeBuilder(AzureResource resource)
    {
        if (_removals.Contains(resource)) return new IgnoreNodeBuilder(resource);
        if (resource is ASP asp && _removals.OfType<App>()
                .Any(x => x.ServerFarmId!.Equals(asp.Id, StringComparison.InvariantCultureIgnoreCase)))

        {
            return new IgnoreNodeBuilder(resource);
        }

        return null;
    }

    public AzureResource ReplacementFor(AzureResource resource)
    {
        var replacement = resource;
        while (_replacements.ContainsKey(replacement))
        {
            replacement = _replacements[replacement];
        }

        return replacement;
    }

    public bool DisplayLink(ResourceLink link)
    {
        return !_noInfer || link.FlowEmphasis != FlowEmphasis.Inferred;
    }
}