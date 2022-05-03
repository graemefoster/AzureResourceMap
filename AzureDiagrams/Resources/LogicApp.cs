using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class LogicApp : AzureResource, ICanBeAccessedViaAHostName
{
    public override string Image => "img/lib/azure2/integration/Logic_Apps.svg";

    public string? AccessEndpoint { get; set; } = default!;
    public string[] Connections { get; set; } = default!;

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        Connections = full["properties"]!["parameters"]?["$connections"]?["value"]?
            .ToObject<Dictionary<string, JObject>>()?
            .Values.Select(x => x.Value<string>("connectionId")!).ToArray() ?? Array.Empty<string>();

        AccessEndpoint = full["properties"]!.Value<string>("accessEndpoint");

        return base.Enrich(full, additionalResources);
    }

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return AccessEndpoint?.Contains(hostname.ToLowerInvariant(), StringComparison.InvariantCultureIgnoreCase) ??
               false;
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        Connections.Select(c =>
                allResources.OfType<LogicAppConnector>()
                    .Single(x => x.Id.Equals(c, StringComparison.InvariantCultureIgnoreCase)))
            .ForEach(c => CreateFlowTo(c, "uses", Plane.Runtime));
        base.BuildRelationships(allResources);
    }
}