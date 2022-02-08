using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AzureDiagrams.Resources.Retrievers.Extensions;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources.Retrievers;

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

        var additionalResources = AdditionalResourcesInternal().ToDictionary(x => x.key,
            x => client.GetAzResourceAsync<JObject>($"{basicResource.Id}/{x.suffix}", x.version ?? _apiVersion,
                x.method).Result);

        JObject? azureResource = default;
        if (_fetchFullResource)
        {
            azureResource = await client.GetAzResourceAsync<JObject>(basicResource.Id, _apiVersion, HttpMethod.Get);
        }

        var additionalResourcesEnhanced =
            AdditionalResourcesInternalEnhanced(basicResource, additionalResources, azureResource).ToDictionary(
                x => x.key,
                x => client.GetAzResourceAsync<JObject>(
                    $"{basicResource.Id}/{x.api}", x.version ?? _apiVersion,
                    x.method).Result);

        var additionalResourcesCustom = await AdditionalResourcesCustom(basicResource, additionalResources, azureResource);

        additionalResources = new Dictionary<string, JObject>(additionalResources.Concat(additionalResourcesEnhanced).Concat(additionalResourcesCustom));

        if (!_fetchFullResource)
        {
            return await BuildResource(_basicAzureResourceJObject, additionalResources);
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        var resource = await BuildResource(azureResource!, additionalResources);
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
    private IEnumerable<(string key, HttpMethod method, string suffix, string? version)> AdditionalResourcesInternal()
    {
        foreach (var extension in _extensions)
        {
            if (extension.ApiCall != null) yield return extension.ApiCall.Value;
        }

        foreach (var customResource in AdditionalResources())
        {
            yield return customResource;
        }
    }

    /// <summary>
    ///     If you have completely custom requirements for additional information, implement it here
    /// </summary>
    /// <returns></returns>
    protected virtual Task<Dictionary<string, JObject>> AdditionalResourcesCustom(BasicAzureResourceInfo basicInfo,
        Dictionary<string, JObject> initialResources, JObject? fullResource)
    {
        return Task.FromResult(new Dictionary<string, JObject>());
    }

    /// <summary>
    ///     Provide any additional resources you need to enrich your object here.
    ///     An example would be the config of a web-app, or the diagnostics settings against an ASP.
    ///     A null version will use the same api version as the original resource.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<(string key, HttpMethod method, string api, string? version)>
        AdditionalResourcesInternalEnhanced(BasicAzureResourceInfo basicInfo,
            Dictionary<string, JObject> initialResources, JObject? fullResource)
    {
        foreach (var customResource in AdditionalResourcesEnhanced(basicInfo, initialResources, fullResource))
        {
            yield return customResource;
        }
    }

    protected virtual IEnumerable<(string key, HttpMethod method, string suffix, string? version)> AdditionalResources()
    {
        yield break;
    }

    /// <summary>
    /// A more dynamic way to retrive resources where you can build APIs based on the initial responses from other API calls.
    /// </summary>
    /// <param name="basicInfo"></param>
    /// <param name="additionalResources"></param>
    /// <returns></returns>
    protected virtual IEnumerable<(string key, HttpMethod method, string api, string? version)>
        AdditionalResourcesEnhanced(BasicAzureResourceInfo basicInfo, Dictionary<string, JObject> additionalResources,
            JObject? fullResource)
    {
        yield break;
    }


    private async Task<AzureResource> BuildResource(JObject resource, Dictionary<string, JObject> additionalResources)
    {
        var resourceRepresentation = resource.ToObject<T>()!;
        resourceRepresentation.Extensions = _extensions;
        await resourceRepresentation.Enrich(resource, additionalResources);
        _extensions.ForEach(x =>
        {
            try
            {
                x.Enrich(resourceRepresentation, resource, additionalResources);
            }
            catch (Exception e)
            {
                throw new DiagramException(
                    $"Failed to run extension {x.GetType().Name} on resource {resourceRepresentation.Id}", e);
            }
        });
        return resourceRepresentation;
    }
}