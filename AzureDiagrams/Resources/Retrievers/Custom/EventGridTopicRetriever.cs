using System.Collections.Generic;
using System.Net.Http;
using DrawIo.Azure.Core.Resources.Retrievers.Extensions;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers.Custom;

public class EventGridTopicRetriever : ResourceRetriever<EventGridTopic>
{
    public const string Subscriptions = "subscriptions";

    public EventGridTopicRetriever(JObject basicAzureResourceJObject) : base(basicAzureResourceJObject,
        fetchFullResource: true, apiVersion: "2021-06-01-preview",
        extensions: new IResourceExtension[]
            { new DiagnosticsExtensions(), new PrivateEndpointExtensions(), new ManagedIdentityExtension() })
    {
    }

    protected override IEnumerable<(string key, HttpMethod method, string suffix, string? version)>
        AdditionalResources()
    {
        yield return (Subscriptions, HttpMethod.Get, "providers/Microsoft.EventGrid/eventSubscriptions", null);
    }
}