using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DrawIo.Azure.Core.Resources.Retrievers;

public static class AzureHttpEx
{
    public static async IAsyncEnumerable<T> GetAzResourcesAsync<T>(this HttpClient httpClient, string uri, string apiVersion) 
    {
        ArmClient.AzureList<T>? results = null;
        do
        {
            results = results == null
                ? await GetAzResourceAsync<ArmClient.AzureList<T>>(httpClient, uri, apiVersion, HttpMethod.Get)
                : await GetAzResourceAsync<ArmClient.AzureList<T>>(httpClient,   httpClient.BaseAddress!.MakeRelativeUri(new Uri(results.NextLink!)).ToString(), null, HttpMethod.Get);
            
            foreach (var item in results.Value)
            {
                yield return item;
            }
        } while (results.NextLink != null);
    }

    public static async Task<T> GetAzResourceAsync<T>(this HttpClient httpClient, string uri, string? apiVersion, HttpMethod? method = null)
    {
        var apiVersionQueryString = apiVersion == null ? "" : (uri.Contains("?") ? "&" : "?") + $"api-version={apiVersion}";
        var resourceUri = $"{uri}{apiVersionQueryString}";

        var request = new HttpRequestMessage(method ?? HttpMethod.Get, resourceUri);
        var httpResponseMessage = await httpClient.SendAsync(request);
        var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var response = JsonConvert.DeserializeObject<T>(responseContent)!;
            return response;
        }

        throw new HttpRequestException($"Failed to fetch {uri}: {responseContent}");
    }
}