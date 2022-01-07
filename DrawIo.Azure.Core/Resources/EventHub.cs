using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class EventHub : AzureResource, ICanBeExposedByPrivateEndpoints
{
    public override string Image => "img/lib/azure2/analytics/Event_Hubs.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        PrivateEndpointConnections =
            full["properties"]!["privateEndpointConnections"]
                ?.Select(x => x["properties"]!["privateEndpoint"]!.Value<string>("id")!).ToArray() ??
            Array.Empty<string>();
        return base.Enrich(full, additionalResources);
    }

    public string[] PrivateEndpointConnections { get; private set; } = default!;

    public bool AccessedViaPrivateEndpoint(PrivateEndpoint privateEndpoint)
    {
        return PrivateEndpointConnections.Any(x => x.Equals(privateEndpoint.Id, StringComparison.InvariantCultureIgnoreCase));
    }
}
