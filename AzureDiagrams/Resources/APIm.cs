using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureDiagrams.Resources.Retrievers.Custom;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class APIm : AzureResource, ICanBeAccessedViaAHostName, ICanInjectIntoASubnet
{
    public override string Image => "img/lib/azure2/app_services/API_Management_Services.svg";

    public string[] Backends { get; set; } = default!;

    public string[] HostNames { get; set; } = default!;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return HostNames.Any(hn => string.Compare(hostname, hn, StringComparison.InvariantCultureIgnoreCase) == 0);
    }

    public string[] PublicIpAddresses { get; private set; } = default!;
    public string[] SubnetIdsIAmInjectedInto { get; private set; } = default!;

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        HostNames = full["properties"]!["hostnameConfigurations"]!.Select(x => x.Value<string>("hostName")!).ToArray();

        Backends = additionalResources[ApimServiceResourceRetriever.BackendList]!["value"]
            ?.Select(x => x["properties"]!.Value<string>("url")!)
            .Select(x => new Uri(x).Host)
            .ToArray() ?? Array.Empty<string>();

        PublicIpAddresses = full["properties"]!["publicIPAddresses"]?
                                .Select(x => x.Value<string>()!).ToArray() ??
                            Array.Empty<string>();

        var vnetConfig = full["properties"]!["virtualNetworkConfiguration"]!;
        var subnet = vnetConfig.Type == JTokenType.Null ? null : vnetConfig!.Value<string>("subnetResourceId");
        SubnetIdsIAmInjectedInto = subnet != null ? new[] { subnet! } : Array.Empty<string>();

        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        Backends.ForEach(x => this.CreateFlowToHostName(allResources, x, "calls", Plane.Runtime));
        base.BuildRelationships(allResources);
    }
}