using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class IpConfigurations
{
    public IpConfigurations(JObject jObject)
    {
        PublicIpAddresses = jObject["properties"]!["ipConfigurations"]!
            .Select(x =>
                x["properties"]!["publicIPAddress"] != null
                    ? x["properties"]!["publicIPAddress"]!.Value<string>("id")!.ToLowerInvariant()
                    : null)
            .Where(x => x != null)
            .Select(x => x!.ToLowerInvariant())
            .ToArray()!;

        PrivateIpAddresses = jObject["properties"]!["ipConfigurations"]!
            .Select(x => x["properties"]!.Value<string>("privateIPAddress"))
            .Where(x => x != null)
            .Select(x => x!)
            .ToArray();

        SubnetAttachments = jObject["properties"]!["ipConfigurations"]!
            .Select(x => x["properties"]!["subnet"]!.Value<string>("id")!.ToLowerInvariant())
            .ToArray();

        HostNames = jObject["properties"]!["ipConfigurations"]!
            .SelectMany(x =>
                x["properties"]!["privateLinkConnectionProperties"]?["fqdns"]?.Values<string>() ??
                Array.Empty<string>())
            .Select(x => x!.ToLowerInvariant())
            .ToArray();
    }

    public string[] PrivateIpAddresses { get; set; }

    public string[] SubnetAttachments { get; set; }

    public string[] HostNames { get; set; }

    public string[] PublicIpAddresses { get; set; }

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return HostNames.Contains(hostname.ToLowerInvariant());
    }
}