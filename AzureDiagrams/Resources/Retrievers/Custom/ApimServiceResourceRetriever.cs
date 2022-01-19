using System.Collections.Generic;
using System.Net.Http;
using DrawIo.Azure.Core.Resources.Retrievers.Extensions;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers.Custom;

/// <summary>
///     Difficult to dive into all operations. So for the moment it only looks at Backends to build relationships.
/// </summary>
public class ApimServiceResourceRetriever : ResourceRetriever<APIm>
{
    public const string BackendList = "backends";

    public ApimServiceResourceRetriever(JObject basicAzureResourceJObject) : base(basicAzureResourceJObject,
        "2021-08-01", true,
        extensions: new IResourceExtension[]
            { new DiagnosticsExtensions(), new PrivateEndpointExtensions(), new ManagedIdentityExtension() })
    {
    }

    protected override IEnumerable<(string key, HttpMethod method, string suffix, string? version)>
        AdditionalResources()
    {
        yield return (BackendList, HttpMethod.Get, BackendList, null);
    }
}