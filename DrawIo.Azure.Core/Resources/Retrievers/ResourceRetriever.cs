using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers;

public class ResourceRetriever<T> : IRetrieveResource where T : AzureResource
{
    private readonly string _apiVersion;
    private readonly JObject _basicAzureResourceJObject;
    private readonly bool _fetchFullResource;

    public ResourceRetriever(JObject basicAzureResourceJObject, string apiVersion = "2021-02-01",
        bool fetchFullResource = false)
    {
        _basicAzureResourceJObject = basicAzureResourceJObject;
        _apiVersion = apiVersion;
        _fetchFullResource = fetchFullResource;
    }

    public async Task<AzureResource> FetchResource(HttpClient client)
    {
        if (!_fetchFullResource)
            return _basicAzureResourceJObject.ToObject<T>()!;

        var basicResource = _basicAzureResourceJObject.ToObject<BasicAzureResourceInfo>()!;
        var azureResource = await client.GetAzResourceAsync<JObject>(basicResource.Id, _apiVersion, HttpMethod.Get);

        var additionalResources = AdditionalResources().ToDictionary(x => x.suffix,
            x => client.GetAzResourceAsync<JObject>($"{basicResource.Id}/{x.suffix}", _apiVersion,
                x.method).Result);
        return await BuildResource(azureResource, additionalResources);
    }

    protected virtual IEnumerable<(HttpMethod method, string suffix)> AdditionalResources()
    {
        yield break;
    }


    private async Task<AzureResource> BuildResource(JObject resource, Dictionary<string, JObject> additionalResources)
    {
        var resourceRepresentation = resource.ToObject<T>()!;
        await resourceRepresentation.Enrich(resource, additionalResources);
        return resourceRepresentation;
    }
}