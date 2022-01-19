using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Resources.Retrievers.Custom;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class EventGridDomain : AzureResource, ICanBeAccessedViaAHostName
{
    public override string Image => "img/lib/azure2/integration/Event_Grid_Domains.svg";

    public Dictionary<string, JObject> Subscriptions { get; set; } = default!;

    public string HostName { get; private set; } = default!;

    public string[] Topics { get; private set; } = default!;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return HostName.Equals(hostname, StringComparison.InvariantCultureIgnoreCase);
    }

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        Topics = additionalResources[EventGridDomainRetriever.Topics]
            ["value"]!.Select(x => x!.Value<string>("name")!).ToArray();

        Subscriptions = Topics.ToDictionary(t => t, t => additionalResources[$"{t}-subscriptions"]);

        HostName = full["properties"]!.Value<string>("endpoint")!.GetHostNameFromUrlString();

        return base.Enrich(full, additionalResources);
    }

    public override IEnumerable<AzureResource> DiscoverNewNodes()
    {
        return Topics.Select(x =>
        {
            var eventGridTopic = new EventGridTopic
            {
                Id = $"{Id}/topics/{x}",
                Name = x,
                Subscriptions = Subscriptions[x]
            };
            OwnsResource(eventGridTopic);
            return eventGridTopic;
        }).ToArray();
    }
}