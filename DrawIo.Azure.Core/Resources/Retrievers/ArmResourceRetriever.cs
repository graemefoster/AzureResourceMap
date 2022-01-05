using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DrawIo.Azure.Core.Resources.Retrievers;

public class ArmResourceRetriever
{
    private readonly HttpClient _httpClient;

    public ArmResourceRetriever(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<AzureResource>> Retrieve(string subscriptionId, string resourceGroup)
    {
        var directResources = await _httpClient.GetAzResourceAsync<AzureList<BasicAzureResourceInfo>>(
            $"/subscriptions/{subscriptionId}/resources?$filter=resourceGroup eq '{resourceGroup}'", "2020-10-01");
        
        var allResources = directResources.Value.Select(GetResourceRetriever)
            .Select(r => r.FetchResource(_httpClient));

        return await Task.WhenAll(allResources);
    }

    private IRetrieveResource GetResourceRetriever(BasicAzureResourceInfo basicAzureResourceInfo)
    {
        return basicAzureResourceInfo.Type.ToLowerInvariant() switch
        {
            "microsoft.web/sites" => new AppResourceRetriever(basicAzureResourceInfo),
            _ => new ResourceRetriever(basicAzureResourceInfo)
        };
    }

    internal class AzureList<T>
    {
        public string NextLink { get; set; }
        public T[] Value { get; set; }
    }
}

public static class AzureHttpEx
{
    public static async Task<T> GetAzResourceAsync<T>(this HttpClient httpClient, string uri, string apiVersion,
        HttpMethod? method = null)
    {
        var apiVersionQueryString = (uri.Contains("?") ? "&" : "?") + $"api-version={apiVersion}";
        var resourceUri = $"{uri}{apiVersionQueryString}";

        var request = new HttpRequestMessage(method ?? HttpMethod.Get, resourceUri);
        var responseContent = await (await httpClient.SendAsync(request)).Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<T>(responseContent)!;
    }
}