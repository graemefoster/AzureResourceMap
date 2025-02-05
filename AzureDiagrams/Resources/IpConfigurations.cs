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
        var ipConfigurations = jObject["properties"]![propertyName];
        if (ipConfigurations?.Type == JTokenType.Object)
        {
            ipConfigurations = new JArray(ipConfigurations);
        }
        
        PublicIpAddresses = ipConfigurations?
            .Select(x =>
                x["properties"]!["publicIPAddress"] != null
                    ? x["properties"]!["publicIPAddress"]!.Value<string>("id")!.ToLowerInvariant()
                    : null)
            .Where(x => x != null)
            .Select(x => x!.ToLowerInvariant())
            .ToArray() ?? [];

        PrivateIpAddresses = ipConfigurations?
            .Select(x => x["properties"]!.Value<string>("privateIPAddress"))
            .Where(x => x != null)
            .Select(x => x!)
            .ToArray() ?? [];

        SubnetAttachments = ipConfigurations?
            .Select(x => x["properties"]!["subnet"]?.Value<string>("id")!.ToLowerInvariant())
            .Where(x => x != null)
            .Select(x => x!)
            .ToArray() ?? [];

        HostNames = ipConfigurations?
            .SelectMany(x =>
                x["properties"]!["privateLinkConnectionProperties"]?["fqdns"]?.Values<string>() ??
                Array.Empty<string>())
            .Select(x => x!.ToLowerInvariant())
            .ToArray() ?? [];
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