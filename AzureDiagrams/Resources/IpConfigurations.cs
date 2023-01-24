using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class IpConfigurations
{
    public IpConfigurations()
    {
    }

    public static IpConfigurations ForPrivateEndpoint(string ipAddress, string subnetId, string hostName)
    {
        return new IpConfigurations()
        {
            PrivateIpAddresses = new[] { ipAddress },
            SubnetAttachments = new[] { subnetId },
            HostNames = new[] { hostName }
        };
    }

    public IpConfigurations(JObject jObject, string propertyName = "ipConfigurations")
    {
        PublicIpAddresses = jObject["properties"]![propertyName]?
            .Select(x =>
                x["properties"]!["publicIPAddress"] != null
                    ? x["properties"]!["publicIPAddress"]!.Value<string>("id")!.ToLowerInvariant()
                    : null)
            .Where(x => x != null)
            .Select(x => x!.ToLowerInvariant())
            .ToArray() ?? Array.Empty<string>();

        PrivateIpAddresses = jObject["properties"]![propertyName]?
            .Select(x => x["properties"]!.Value<string>("privateIPAddress"))
            .Where(x => x != null)
            .Select(x => x!)
            .ToArray() ?? Array.Empty<string>();

        SubnetAttachments = jObject["properties"]![propertyName]?
            .Select(x => x["properties"]!["subnet"]?.Value<string>("id")!.ToLowerInvariant())
            .Where(x => x != null)
            .Select(x => x!)
            .ToArray() ?? Array.Empty<string>();

        HostNames = jObject["properties"]![propertyName]?
            .SelectMany(x =>
                x["properties"]!["privateLinkConnectionProperties"]?["fqdns"]?.Values<string>() ??
                Array.Empty<string>())
            .Select(x => x!.ToLowerInvariant())
            .ToArray() ?? Array.Empty<string>();
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