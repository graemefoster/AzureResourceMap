using System.Collections.Generic;
using System.Net.Http;
using DrawIo.Azure.Core.Resources.Retrievers.Extensions;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers.Custom;

public class AzureDataFactoryRetriever : ResourceRetriever<ADF>
{
    public const string LinkedServices = "linkedservices";

    public AzureDataFactoryRetriever(JObject basicAzureResourceJObject) : base(basicAzureResourceJObject,
        fetchFullResource: true, apiVersion: "2018-06-01",
        extensions: new IResourceExtension[]
            { new DiagnosticsExtensions(), new PrivateEndpointExtensions(), new ManagedIdentityExtension() })
    {
    }

    protected override IEnumerable<(string key, HttpMethod method, string suffix, string? version)>
        AdditionalResources()
    {
        yield return (LinkedServices, HttpMethod.Get, LinkedServices, null);
    }
}