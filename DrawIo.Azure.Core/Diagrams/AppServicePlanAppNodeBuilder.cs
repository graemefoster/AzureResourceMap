using System.Collections.Generic;
using DrawIo.Azure.Core.Resources;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Diagrams;

public class AppServicePlanAppNodeBuilder : AzureResourceNodeBuilder
{
    private readonly AzureResource _resource;

    public AppServicePlanAppNodeBuilder(App resource) : base(resource)
    {
        _resource = resource;
    }

    protected override IEnumerable<(AzureResource, Node)> CreateNodesInternal(
        IDictionary<AzureResource, AzureResourceNodeBuilder> resourceNodeBuilders)
    {
        yield return (_resource,
            AzureResourceDrawer.CreateSimpleImageNode("img/lib/azure2/app_services/App_Services.svg",
                _resource.Name,
                _resource.InternalId));
    }
}