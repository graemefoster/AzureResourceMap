using AzureDiagrams.Resources;

namespace AzureDiagramGenerator.DrawIo.DiagramAdjustors;

public class CondensedDiagramAdjustor : IDiagramAdjustor
{
    private readonly IDiagramAdjustor _inner;
    private readonly Dictionary<AzureResource, AzureResource> _replacements = new();
    private readonly List<AzureResource> _removals = new();
    private readonly List<ResourceLink> _ignoreLinks = new();

    public CondensedDiagramAdjustor(IDiagramAdjustor inner, AzureResource[] allResources)
    {
        _inner = inner;
        CollapsePrivateEndpoints(allResources);
        CollapseVNetIntegrations(allResources);
        CollapseVirtualMachines(allResources);
        _removals.AddRange(_replacements.Keys);
    }

    private void CollapseVirtualMachines(AzureResource[] allResources)
    {
        foreach (var vm in allResources.OfType<VM>())
        {
            var nics = allResources.OfType<Nic>()
                .Where(x => vm.Nics.Contains(x.Id, StringComparer.InvariantCultureIgnoreCase));
            foreach (var nic in nics)
            {
                _replacements.Add(nic, vm);
            }
            var disk = allResources.OfType<Disk>().SingleOrDefault(x => vm.SystemDiskId.Equals(x.Id, StringComparison.InvariantCultureIgnoreCase));
            if (disk != null)
            {
                _replacements.Add(disk, vm);
            }
        }
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
                
                //the current Nic is the one that will stay on the diagram. Name it to reflect the resource that is accessed
                currentPe.nic.Name = currentPe.pe.ResourceAccessedByMe?.Name ?? currentPe.nic.Name;

                //finally target vnet integration
                if (grouping.Key is AppServiceApp { VNetIntegration: { } } app)
                {
                    _replacements.Add(app.VNetIntegration!, currentPe.nic);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Resource {grouping.Key.Name} is injected into multiple subnets. Unable to correctly condense it to a single subnet. Diagram may look odd.");
                Console.ResetColor();
            }
        }
    }

    private void CollapseVNetIntegrations(AzureResource[] allResources)
    {

        var publicAppWithVNetIntegration = allResources.OfType<AppServiceApp>()
                .Where(x => x.VNetIntegration != null)
                .Where(app => allResources.OfType<PrivateEndpoint>().All(pe => pe.ResourceAccessedByMe != app));

        foreach (var app in publicAppWithVNetIntegration)
        {
            _replacements.Add(app, app.VNetIntegration!);
        }

    }

    public string ImageFor(AzureResource resource)
    {
        if (resource is VNetIntegration vNetIntegration && _replacements.ContainsKey(vNetIntegration.LinkedApp))
        {
            return vNetIntegration.LinkedApp.Image;
        }
        
        return resource switch
        {
            Nic { ConnectedPrivateEndpoint: not null } res =>
                res.ConnectedPrivateEndpoint!.ResourceAccessedByMe?.Image ?? res.Image,
            _ => _inner.ImageFor(resource)
        };
    }

    public AzureResourceNodeBuilder? CreateNodeBuilder(AzureResource resource)
    {
        if (_removals.Contains(resource)) return new IgnoreNodeBuilder(resource);
        
        //Don't draw the ASP if all of its apps are being routed via private endpoint / vnet-integration subnets
        if (resource is AppServicePlan asp && asp.ContainedResources.OfType<AppServiceApp>().All(app => _replacements.ContainsKey(app)))
        {
            return new IgnoreNodeBuilder(resource);
        }

        return _inner.CreateNodeBuilder(resource);
    }

    /// <summary>
    /// look for any new 2-way links. These clutter the diagram and we prefer double arrow-heads.
    /// </summary>
    /// <param name="all"></param>
    public void PostProcess(Dictionary<AzureResource, AzureResourceNodeBuilder> all)
    {
        var realLinks = new HashSet<(AzureResource, AzureResource, Plane)>();
        var originalLinks = new Dictionary<(AzureResource, AzureResource, Plane), ResourceLink>();
        foreach (var resource in all.Keys)
        {
            var from = ReplacementFor(resource);
            foreach (var link in resource.Links)
            {
                if (_inner.DisplayLink(link))
                {
                    var to = ReplacementFor(link.To);
                    if (realLinks.Contains((to, from, link.Plane)))
                    {
                        _ignoreLinks.Add(link);
                        originalLinks[(to, from, link.Plane)].MakeTwoWay();
                    }
                    else
                    {
                        var key = (from, to, link.Plane);
                        realLinks.Add(key);
                        originalLinks[key] = link;
                    }
                }
            }
        }
        
        _inner.PostProcess(all);
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
        return !_ignoreLinks.Contains(link) && _inner.DisplayLink(link);
    }
}