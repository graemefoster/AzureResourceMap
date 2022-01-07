using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class Bastion : AzureResource, ICanInjectIntoASubnet, ICanExposePublicIPAddresses
{
    public override string Image => "img/lib/azure2/networking/Connections.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        SubnetIdsIAmInjectedInto = full["properties"]!["ipConfigurations"]
            ?.Select(x => x["properties"]!["subnet"]!.Value<string>("id")!).ToArray() ?? Array.Empty<string>();
        PublicIpAddresses = full["properties"]!["ipConfigurations"]
            ?.Select(x => x["properties"]!["publicIPAddress"]!.Value<string>("id")!).ToArray() ?? Array.Empty<string>();
        return base.Enrich(full, additionalResources);
    }

    public string[] SubnetIdsIAmInjectedInto { get; private set; }
    public string[] PublicIpAddresses { get; private set; }
}