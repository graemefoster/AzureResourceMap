using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class CognitiveSearch : AzureResource, ICanBeAccessedViaAHostName
{
    public override string Image => "img/lib/azure2/general/Search.svg";

    public string HostName { get; set; } = default!;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return string.Compare(HostName, hostname, StringComparison.InvariantCultureIgnoreCase) == 0;
    }

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        HostName = $"{Name.ToLowerInvariant()}.search.windows.net";
        return base.Enrich(full, additionalResources);
    }
}