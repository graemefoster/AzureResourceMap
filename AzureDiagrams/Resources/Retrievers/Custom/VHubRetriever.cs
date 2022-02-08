using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers.Custom;

public class VHubRetriever : ResourceRetriever<VHub>
{
    public const string VirtualNetworkConnections = "hubVirtualNetworkConnections";
    
    public VHubRetriever(JObject basicAzureResourceJObject) : base(basicAzureResourceJObject, "2021-03-01", true)
    {
    }

    protected override IEnumerable<(string key, HttpMethod method, string suffix, string? version)> AdditionalResources()
    {
        yield return (VirtualNetworkConnections, HttpMethod.Get, "hubVirtualNetworkConnections", "2021-03-01");
    }
}