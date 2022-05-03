using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class Bot : AzureResource
{
    public override string Image => "img/lib/mscae/Bot_Services.svg";

    public string? BotEndpoint { get; set; }

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        BotEndpoint = full["properties"]!.Value<string>("endpoint");
        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        if (BotEndpoint != null)
        {
            if (Uri.TryCreate(BotEndpoint, UriKind.Absolute, out var uri))
            {
                this.CreateFlowToHostName(allResources, uri.Host, "communicates", Plane.Runtime);
            }
        }
        base.BuildRelationships(allResources);
    }
}