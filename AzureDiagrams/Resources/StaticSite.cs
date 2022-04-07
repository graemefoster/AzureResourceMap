using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class StaticSite : AzureResource, ICanBeAccessedViaAHostName
{
    public override string Image => "img/lib/azure2/app_services/App_Service_Domains.svg";

    public string[] EnabledHostNames { get; set; } = default!;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return EnabledHostNames.Any(
            hn => string.Compare(hn, hostname, StringComparison.InvariantCultureIgnoreCase) == 0);
    }


    public override async Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        await base.Enrich(full, additionalResources);
        EnabledHostNames =
            new[] { full["properties"]!.Value<string>("defaultHostname")! }
                .Concat(full["properties"]!["customDomains"]!.Values<string>().Select(x => x!)).ToArray();
    }
}