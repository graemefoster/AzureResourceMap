using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class CognitiveServices : AzureResource, ICanBeAccessedViaAHostName
{
    public override string Image => "img/lib/azure2/ai_machine_learning/Cognitive_Services.svg";

    public string Kind { get; set; } = default!;

    public string[] HostNames { get; set; } = default!;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return HostNames.Contains(hostname.ToLowerInvariant()) 
               || Name.Equals(hostname, StringComparison.InvariantCultureIgnoreCase);
    }

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        HostNames = full["properties"]!["endpoints"]!.ToObject<Dictionary<string, string>>()!.Values
            .Select(x => x.GetHostNameFromUrlString()).ToArray();
        return base.Enrich(full, additionalResources);
    }
}