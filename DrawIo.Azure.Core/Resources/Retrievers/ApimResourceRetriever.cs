using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers;

/// <summary>
/// Difficult to dive into all operations. So for the moment it only looks at Backends to build relationships.
/// </summary>
public class ApimServiceResourceRetriever : ResourceRetriever<APIm>
{
    public const string BackendList = "backends";

    public ApimServiceResourceRetriever(JObject basicAzureResourceJObject) : base(basicAzureResourceJObject, "2021-08-01", true)
    {
    }

    protected override IEnumerable<(HttpMethod method, string suffix, string? version)> AdditionalResources()
    {
        yield return (HttpMethod.Get, BackendList, null);
    }
}