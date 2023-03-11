using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AzureDiagrams.Resources.Retrievers.Extensions;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources.Retrievers.Custom;

public class EventGridDomainRetriever : ResourceRetriever<EventGridDomain>
{
    public const string Topics = "topics";

    public EventGridDomainRetriever(JObject basicAzureResourceJObject) : base(basicAzureResourceJObject,
        apiVersion: "2021-06-01-preview",
        extensions: new IResourceExtension[] { new DiagnosticsExtensions(), new PrivateEndpointExtensions(), new ManagedIdentityExtension() })
    {
    }

    protected override IEnumerable<(string key, HttpMethod method, string suffix, string? version)>
        AdditionalResources()
    {
        yield return (Topics, HttpMethod.Get, Topics, null);
    }

    protected override IEnumerable<(string key, HttpMethod method, string api, string? version)> AdditionalResourcesEnhanced(BasicAzureResourceInfo basicInfo, Dictionary<string, JObject?> additionalResources)
    {
        foreach (var topic in additionalResources[Topics]!
                     ["value"]!.Select(x => x!.Value<string>("name")!))
        {
            yield return ($"{topic}-subscriptions", HttpMethod.Get, $"topics/{topic}/providers/Microsoft.EventGrid/eventSubscriptions", null);
        }
    }
}