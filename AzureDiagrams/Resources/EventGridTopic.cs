using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class EventGridTopic : AzureResource
{
    public JObject? Subscriptions { get; internal set; }
    public override string Image => "img/lib/azure2/integration/Event_Grid_Topics.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        Subscriptions?["value"]!.ForEach(s => HandleSubscription(s["properties"]!, allResources));
        base.BuildRelationships(allResources);
    }

    private void HandleSubscription(JToken jt, IEnumerable<AzureResource> allResources)
    {
        if (jt["destination"]!.Value<string>("endpointType") == "EventHub")
        {
            var ehId = jt["destination"]!["properties"]!.Value<string>("resourceId");
            var eh = allResources.OfType<EventHub>()
                .SingleOrDefault(x => ehId.StartsWith(x.Id, StringComparison.InvariantCultureIgnoreCase));
            if (eh != null)
            {
                CreateFlowTo(eh, "subscription");
            }
        }
    }
}