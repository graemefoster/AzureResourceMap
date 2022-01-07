using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers;

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