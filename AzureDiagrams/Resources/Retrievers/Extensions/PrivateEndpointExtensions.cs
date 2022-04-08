using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources.Retrievers.Extensions;

public class PrivateEndpointExtensions : IResourceExtension
{
    public string[] PrivateEndpointConnections { get; private set; } = default!;

    public (string key, HttpMethod method, string suffix, string? version)? ApiCall => null;

    public Task Enrich(AzureResource resource, JObject raw, Dictionary<string, JObject?> additionalResources)
    {
        //Private endpoints are expressed in a common way across the platform. To generalise I've added the check to AzureResource.
        PrivateEndpointConnections =
            raw["properties"]?["privateEndpointConnections"]
                ?.Select(x => x["properties"]!["privateEndpoint"]!.Value<string>("id")!).ToArray() ??
            Array.Empty<string>();

        return Task.CompletedTask;
    }

    public void BuildRelationships(AzureResource resource, IEnumerable<AzureResource> allResources)
    {
    }

    public bool AccessedViaPrivateEndpoint(PrivateEndpoint privateEndpoint)
    {
        return PrivateEndpointConnections?.Any(x =>
            x.Equals(privateEndpoint.Id, StringComparison.InvariantCultureIgnoreCase)) ?? false;
    }
}