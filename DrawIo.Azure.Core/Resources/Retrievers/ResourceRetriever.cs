using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Resources.Retrievers.Extensions;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers;

public class ResourceRetriever<T> : IRetrieveResource where T : AzureResource
{
    private readonly string _apiVersion;
    private readonly JObject _basicAzureResourceJObject;
    private readonly bool _fetchFullResource;
    private readonly IEnumerable<IResourceExtension> _extensions;

    public ResourceRetriever(
        JObject basicAzureResourceJObject, 
        string apiVersion = "2021-02-01",
        bool fetchFullResource = false,
        IEnumerable<IResourceExtension>? extensions = null)
    {
        _basicAzureResourceJObject = basicAzureResourceJObject;
        _apiVersion = apiVersion;
        _fetchFullResource = fetchFullResource;
        _extensions = extensions ?? Array.Empty<IResourceExtension>();
    }

    public async Task<AzureResource> FetchResource(HttpClient client)
    {
        var basicResource = _basicAzureResourceJObject.ToObject<BasicAzureResourceInfo>()!;

        var additionalResources = AdditionalResources().ToDictionary(x => x.key,
            x => client.GetAzResourceAsync<JObject>($"{basicResource.Id}/{x.suffix}", x.version ?? _apiVersion,
                x.method).Result);

        if (!_fetchFullResource)
            return await BuildResource(_basicAzureResourceJObject, additionalResources);

        var azureResource = await client.GetAzResourceAsync<JObject>(basicResource.Id, _apiVersion, HttpMethod.Get);
        Console.ForegroundColor = ConsoleColor.Yellow;
        var resource = await BuildResource(azureResource, additionalResources);
        Console.WriteLine($"\tProcessed resource {resource.Type}/{resource.Name}");
        Console.ResetColor();
        return resource;
    }

    /// <summary>
    ///     Provide any additional resources you need to enrich your object here.
    ///     An example would be the config of a web-app, or the diagnostics settings against an ASP.
    ///     A null version will use the same api version as the original resource.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<(string key, HttpMethod method, string suffix, string? version)> AdditionalResources()
    {
        foreach (var extension in _extensions)
        {
            if (extension.ApiCall != null) yield return extension.ApiCall.Value;
        }
    }


    private async Task<AzureResource> BuildResource(JObject resource, Dictionary<string, JObject> additionalResources)
    {
        var resourceRepresentation = resource.ToObject<T>()!;
        resourceRepresentation.Extensions = _extensions;
        await resourceRepresentation.Enrich(resource, additionalResources);
        _extensions.ForEach(x => x.Enrich(resourceRepresentation, resource, additionalResources));
        return resourceRepresentation;
    }
}