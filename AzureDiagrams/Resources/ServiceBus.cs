using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class ServiceBus : AzureResource, ICanBeAccessedViaAHostName
{
    public override string Image => "img/lib/mscae/Service_Bus.svg";

    public string[] HostNames { get; private set; } = default!;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return HostNames.Contains(hostname, StringComparer.InvariantCultureIgnoreCase);
    }

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        HostNames = new[] { full["properties"]!.Value<string>("serviceBusEndpoint")!.GetHostNameFromUrlString() };
        return base.Enrich(full, additionalResources);
    }
}