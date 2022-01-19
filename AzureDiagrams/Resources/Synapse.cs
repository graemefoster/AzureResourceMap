using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Resources.Retrievers.Custom;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class Synapse : AzureResource, ICanBeAccessedViaAHostName
{
    public override string Image => "img/lib/azure2/databases/Data_Factory.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        HostNames = full["properties"]!["connectivityEndpoints"]!.ToObject<Dictionary<string, string>>()!.Values.Select(x => x.GetHostNameFromUrlStringOrNull() ?? x).ToArray();
        return base.Enrich(full, additionalResources);
    }

    private string[] HostNames { get; set; } = default!;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return HostNames.Contains(hostname, StringComparer.InvariantCultureIgnoreCase);
    }
}