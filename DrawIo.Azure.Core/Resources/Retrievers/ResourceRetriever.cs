using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers;

public class ResourceRetriever<T>  : IRetrieveResource where T:AzureResource
{
    private readonly BasicAzureResourceInfo _basicAzureResourceInfo;

    public ResourceRetriever(BasicAzureResourceInfo basicAzureResourceInfo)
    {
        _basicAzureResourceInfo = basicAzureResourceInfo;
    }

    public virtual string ApiVersion => "2020-11-01";

    public virtual bool FetchFull => false;

    public async Task<AzureResource> FetchResource(HttpClient client)
    {
        var azureResource =
            await client.GetAzResourceAsync<JObject>(_basicAzureResourceInfo.Id, ApiVersion, HttpMethod.Get);
        
        var additionalResources = AdditionalResources().ToDictionary(x => x.suffix,
            x => client.GetAzResourceAsync<JObject>($"{_basicAzureResourceInfo.Id}/{x.suffix}", ApiVersion, HttpMethod.Post).Result);
        
        return BuildResource(azureResource, additionalResources);
    }

    protected virtual IEnumerable<(HttpMethod method, string suffix)> AdditionalResources()
    {
        yield break;
    }


    protected virtual AzureResource BuildResource(BasicAzureResourceInfo basicAzureResourceInfo, JObject resource, Dictionary<string, JObject> additionalResources)
    {
        return resource.ToObject<T>()!;
    }
}