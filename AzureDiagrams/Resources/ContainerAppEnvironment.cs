using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

internal class ContainerAppEnvironment : AzureResource, ICanWriteToLogAnalyticsWorkspaces
{
    public override string Image => "img/lib/mscae/Kubernetes_Services.svg";
    public string? LogAnalyticsCustomerId { get; private set; }

    public bool DoYouWriteTo(string customerId)
    {
        return LogAnalyticsCustomerId == customerId;
    }

    public void CreateFlowBackToMe(LogAnalyticsWorkspace workspace)
    {
        CreateFlowTo(workspace, "logs", Plane.Diagnostics);
    }

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        LogAnalyticsCustomerId = full["properties"]!["appLogsConfiguration"]?["logAnalyticsConfiguration"]
            ?.Value<string>("customerId");
        return base.Enrich(full, additionalResources);
    }

    public void DiscoveredContainerApp(ContainerApp containerApp)
    {
        OwnsResource(containerApp);
    }
}