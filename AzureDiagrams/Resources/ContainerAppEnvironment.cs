using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

internal class ContainerAppEnvironment : AzureResource, ICanWriteToLogAnalyticsWorkspaces, ICanInjectIntoASubnet
{
    private string[] _subnets;
    public override string Image => "img/lib/azure2/other/Container_App_Environments.svg";
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
        var jToken = full["properties"]!
            ["appLogsConfiguration"]?
            ["logAnalyticsConfiguration"];

        jToken = jToken?.Type == JTokenType.Null ? null : jToken;
        
        LogAnalyticsCustomerId = jToken?.Value<string>("customerId");

        var subnet = full["properties"]!["vnetConfiguration"]?.Value<string>("infrastructureSubnetId");
        _subnets = subnet != null ? [subnet] : [];
        
        return base.Enrich(full, additionalResources);
    }

    public void DiscoveredContainerApp(ContainerApp containerApp)
    {
        OwnsResource(containerApp);
    }

    public string[] SubnetIdsIAmInjectedInto => _subnets;
}