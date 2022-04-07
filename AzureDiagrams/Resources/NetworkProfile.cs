using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class NetworkProfile : AzureResource
{
    private string[] _linkedContainers = default!;
    public string[] SubnetIds { get; private set; } = default!;

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        _linkedContainers = full["properties"]!["containerNetworkInterfaces"]
            ?.Select(x => x["properties"]!["container"]?.Value<string>("id"))
            .Where(x => x != null).Select(x => x!).Distinct().ToArray() ?? Array.Empty<string>();

        SubnetIds = full["properties"]!["containerNetworkInterfaceConfigurations"]
                        ?.SelectMany(x =>
                            x["properties"]!["ipConfigurations"]!.Select(y =>
                                y["properties"]!["subnet"]!.Value<string>("id")!)).Distinct().ToArray() ??
                    Array.Empty<string>();

        return base.Enrich(full, additionalResources);
    }
}