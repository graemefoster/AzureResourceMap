using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DrawIo.Azure.Core.Resources.Retrievers;

public static class AzureHttpEx
{
    public static async Task<T> GetAzResourceAsync<T>(this HttpClient httpClient, string uri, string apiVersion,
        HttpMethod? method = null)
    {
        var apiVersionQueryString = (uri.Contains("?") ? "&" : "?") + $"api-version={apiVersion}";
        var resourceUri = $"{uri}{apiVersionQueryString}";

        var request = new HttpRequestMessage(method ?? HttpMethod.Get, resourceUri);
        var httpResponseMessage = await httpClient.SendAsync(request);
        var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

        if (httpResponseMessage.IsSuccessStatusCode) return JsonConvert.DeserializeObject<T>(responseContent)!;

        throw new HttpRequestException($"Failed to fetch {uri}: {responseContent}");
    }
}