using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class AppGateway : AzureResource, ICanBeAccessedViaHttp, ICanInjectIntoASubnet, ICanExposePublicIPAddresses
{
    public override string Image => "img/lib/azure2/networking/Application_Gateways.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        PublicIpAddresses = full["properties"]!["frontendIPConfigurations"]?.Select(x => x["properties"]!["publicIPAddress"]?.Value<string>("id")).Where(x => x != null).Select(x => x!).ToArray() ?? Array.Empty<string>();
        Subnets = full["properties"]!["gatewayIPConfigurations"]?.Select(x => x["properties"]!["subnet"]?.Value<string>("id")).Where(x => x != null).Select(x => x!).ToArray() ?? Array.Empty<string>();
        return base.Enrich(full, additionalResources);
    }

    public string[] PublicIpAddresses { get; set; } = default!;
    public string[] Subnets { get; set; } = default!;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        //TODO - work out how to get the hostnames.
        return false;
    }

    public string[] SubnetIdsIAmInjectedInto => Subnets;
}