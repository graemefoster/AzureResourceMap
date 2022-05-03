using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureDiagrams.Resources.Retrievers.Custom;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class EventGridTopic : AzureResource
{
    public JObject? Subscriptions { get; internal set; }
    public override string Image => "img/lib/azure2/integration/Event_Grid_Topics.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        Subscriptions = additionalResources[EventGridTopicRetriever.Subscriptions];
        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        Subscriptions?["value"]!.ForEach(s => HandleSubscription(s["properties"]!, allResources));
        base.BuildRelationships(allResources);
    }

    private void HandleSubscription(JToken jt, IEnumerable<AzureResource> allResources)
    {
        switch (jt["destination"]!.Value<string>("endpointType"))
        {
            case "EventHub":
            case "AzureFunction":
            case "ServiceBus":
            case "ServiceTopic":
            case "StorageQueue":
            case "HybridConnection":
                HandleResourceSubscription(jt, allResources);
                break;
            case "WebHook":
                HandleUrlSubscription(jt, allResources);
                break;
        }
    }

    private void HandleUrlSubscription(JToken jt, IEnumerable<AzureResource> allResources)
    {
        var hostName = jt["destination"]!["properties"]!.Value<string>("endpointBaseUrl")!.GetHostNameFromUrlString();
        var resource = allResources.OfType<ICanBeAccessedViaAHostName>()
            .SingleOrDefault(x => x.CanIAccessYouOnThisHostName(hostName));
        if (resource != null)
        {
            CreateFlowTo((AzureResource)resource, "subscription", Plane.Runtime);
        }
    }

    private void HandleResourceSubscription(JToken jt, IEnumerable<AzureResource> allResources)
    {
        var resourceId = jt["destination"]!["properties"]!.Value<string>("resourceId")!;
        var resource =
            allResources.SingleOrDefault(x => resourceId.StartsWith(x.Id, StringComparison.InvariantCultureIgnoreCase));
        if (resource != null)
        {
            CreateFlowTo(resource, "subscription", Plane.Runtime);
        }
    }
}