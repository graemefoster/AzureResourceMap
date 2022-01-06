using System;
using System.Collections.Generic;
using System.Linq;
using DrawIo.Azure.Core.Diagrams;

namespace DrawIo.Azure.Core.Resources;

internal class ASP : AzureResource
{
    public string Kind { get; set; } = default!;
    public override string Image => "img/lib/azure2/app_services/App_Service_Plans.svg";
    public App[] ContainedApps { get; private set; } = default!;

    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new AppServicePlanDiagramResourceBuilder(this);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        ContainedApps = allResources.OfType<App>().Where(x =>
            string.Equals(Id, x.ServerFarmId, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        ContainedApps.ForEach(x => x.ContainedByAnotherResource = true);
    }
}