using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

internal class AppInsights : AzureResource
{
    public override string Image => "img/lib/azure2/devops/Application_Insights.svg";
    public string InstrumentationKey { get; private set; } = default!;
    public string? WorkspaceResourceId { get; private set; }

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        InstrumentationKey = full["properties"]!.Value<string>("InstrumentationKey")!;
        WorkspaceResourceId = full["properties"]!.Value<string>("WorkspaceResourceId");
        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        if (WorkspaceResourceId != null)
        {
            var workspace = allResources.OfType<LogAnalyticsWorkspace>().SingleOrDefault(x =>
                string.Compare(WorkspaceResourceId, x.Id, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (workspace != null) CreateFlowTo(workspace, "logs", Plane.Diagnostics);
        }
        base.BuildRelationships(allResources);
    }
}