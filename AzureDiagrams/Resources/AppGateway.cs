using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class AppGateway : AzureResource, ICanBeAccessedViaAHostName, ICanInjectIntoASubnet, ICanExposePublicIPAddresses
{
    public override string Image =>  "img/lib/azure2/networking/Application_Gateways.svg";

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return Hostnames.Contains(hostname, StringComparer.InvariantCultureIgnoreCase);
    }

    public string[] Hostnames { get; private set; } = default!;

    public string[] HostnamesITryToContact { get; private set; } = default!;

    public string[] PublicIpAddresses { get; private set; } = default!;

    public string[] SubnetIdsIAmInjectedInto { get; private set; } = default!;

    public bool IsWaf { get; set; }

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        IsWaf = full["properties"]!["sku"]!.Value<string>("tier")!.Contains("waf",
            StringComparison.CurrentCultureIgnoreCase);

        SubnetIdsIAmInjectedInto = full["properties"]!["gatewayIPConfigurations"]?
            .Select(x => x["properties"]!["subnet"]!.Value<string>("id")!.ToLowerInvariant())
            .ToArray() ?? Array.Empty<string>();

        Hostnames = full["properties"]!["httpListeners"]?
            .SelectMany(x =>
                x["properties"]!["hostNames"]?.Values<string>().Select(hn => hn!.ToLowerInvariant()) ??
                Array.Empty<string>())
            .ToArray() ?? Array.Empty<string>();

        HostnamesITryToContact = full["properties"]!["backendAddressPools"]?
            .SelectMany(x =>
                x["properties"]!["backendAddresses"]?.Select(ba => ba["fqdn"]?.Value<string>()?.ToLowerInvariant())
                    .Where(ba => ba != null).Select(ba => ba!) ?? Array.Empty<string>())
            .ToArray() ?? Array.Empty<string>();

        PublicIpAddresses = full["properties"]!["frontendIPConfigurations"]?
            .Select(x => x["properties"]!["publicIPAddress"]?.Value<string>("id"))
            .Where(x => x != null)
            .Select(x => x!)
            .ToArray() ?? Array.Empty<string>();

        return base.Enrich(full, additionalResources);
    }


    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        HostnamesITryToContact.ForEach(x => this.CreateFlowToHostName(allResources, x, "Backend", Plane.Runtime));
        base.BuildRelationships(allResources);
    }
}