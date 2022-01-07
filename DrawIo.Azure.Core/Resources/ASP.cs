using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Diagrams;
using DrawIo.Azure.Core.Resources.Retrievers;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class ASP : AzureResource
{
    private string? _diagnosticsWorkspaceId;
    public override string Image => "img/lib/azure2/app_services/App_Service_Plans.svg";
    

    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new AppServicePlanAppNodeBuilder(this);
    }

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        _diagnosticsWorkspaceId = additionalResources[AppServicePlanResourceRetriever.DiagnosticSettings]["value"]?[0]?["properties"]?.Value<string>("workspaceId");
        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var apps = allResources.OfType<App>().Where(x =>
            string.Equals(Id, x.ServerFarmId, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        apps.ForEach(OwnsResource);

        if (_diagnosticsWorkspaceId != null)
        {
            var workspace = allResources.OfType<LogAnalyticsWorkspace>().SingleOrDefault(x =>
                string.Compare(_diagnosticsWorkspaceId, x.Id, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (workspace != null)
            {
                CreateFlowTo(workspace, "diagnostics");
            }
        }
    }
}