using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class LogAnalyticsWorkspace : AzureResource
{
    public override string Image => "img/lib/azure2/analytics/Log_Analytics_Workspaces.svg";
    public string CustomerId { get; private set; } = default!;

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        CustomerId = full["properties"]!.Value<string>("customerId")!;
        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<ICanWriteToLogAnalyticsWorkspaces>().Where(x => x.DoYouWriteTo(CustomerId)).ForEach(x => x.CreateFlowBackToMe(this));
    }
}