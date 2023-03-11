using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AzureDiagrams.Resources.Retrievers;

public static class AzureHttpEx
{
    public static async IAsyncEnumerable<T> GetAzResourcesAsync<T>(this HttpClient httpClient, string uri,
        string apiVersion)
    {
        ArmClient.AzureList<T>? results = null;
        do
        {
            results = results == null
                ? await GetAzResourceAsync<ArmClient.AzureList<T>>(httpClient, uri, apiVersion, HttpMethod.Get)
                : await GetAzResourceAsync<ArmClient.AzureList<T>>(httpClient,
                    httpClient.BaseAddress!.MakeRelativeUri(new Uri(results.NextLink!)).ToString(), null,
                    HttpMethod.Get);

            foreach (var item in results.Value)
            {
                yield return item;
            }
        } while (results.NextLink != null);
    }

    public static async IAsyncEnumerable<T> GetAzResourcesQueryAsync<T>(
        this HttpClient httpClient,
        Guid subscriptionId,
        object query,
        string apiVersion)
    {
        ArmClient.AzureResourceGraphQueryResult<T>? results = null;
        do
        {
            var rgQuery = (string? skipToken) => GetAzResourceAsync<ArmClient.AzureResourceGraphQueryResult<T>>(
                httpClient,
                "/providers/Microsoft.ResourceGraph/resources",
                apiVersion,
                HttpMethod.Post,
                new StringContent(JsonConvert.SerializeObject(
                    new
                    {
                        query = query,
                        subscriptions = new[] { subscriptionId },
                        options= new RgQueryOptions {
                            SkipToken =  skipToken
                        }
                    }), Encoding.UTF8, "application/json"));

            results = results == null ? await rgQuery(null) : await rgQuery(results.SkipToken);

            foreach (var item in results.Data)
            {
                yield return item;
            }
        } while (results.SkipToken != null);
    }

    public static async Task<T> GetAzResourceAsync<T>(this HttpClient httpClient, string uri, string? apiVersion,
        HttpMethod? method = null, HttpContent? content = null
    )
    {
        var apiVersionQueryString =
            apiVersion == null ? "" : (uri.Contains("?") ? "&" : "?") + $"api-version={apiVersion}";
        var resourceUri = $"{uri}{apiVersionQueryString}";

        var request = new HttpRequestMessage(method ?? HttpMethod.Get, resourceUri);
        if (content != null)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = content;
        }

        var httpResponseMessage = await httpClient.SendAsync(request);
        var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

        httpResponseMessage.EnsureSuccessStatusCode();
        var response = JsonConvert.DeserializeObject<T>(responseContent)!;
        return response;
    }

    class RgQueryOptions
    {
        [JsonProperty("$skipToken")] public string? SkipToken { get; set; }
    }
}