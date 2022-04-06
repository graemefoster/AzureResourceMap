using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class P2S : AzureResource
{
    public override string? Image => "img/lib/mscae/VPN_Gateway.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        VHubId = full["properties"]!["virtualHub"]?.Value<string>("id");
        return base.Enrich(full, additionalResources);
    }

    public string? VHubId { get; set; }
}