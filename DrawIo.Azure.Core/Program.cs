using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using DrawIo.Azure.Core.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var directoryName = @"C:\Users\graemefoster\Documents\LINQPad Queries\AzureResourceManager\";
            var responseCacheFile =
                Path.Combine(
                    directoryName,
                    "responseCache.json");

            var responseCache = File.Exists(responseCacheFile)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(responseCacheFile))
                : new Dictionary<string, string>();

            var azCliClientId = "04b07795-8ddb-461a-bbee-02f9e1bf7b46";

            var tokenClient = Microsoft.Identity.Client.PublicClientApplicationBuilder
                .Create(azCliClientId)
                .WithTenantId("microsoft.onmicrosoft.com")
                .WithClientName("Linqpad")
                .WithDefaultRedirectUri()
                .Build();

            var fetchToken = false;
            var token = new AzureCliCredential().GetToken(
                new TokenRequestContext(new[] {"https://management.azure.com/"}));


            var httpClient = new HttpClient(new HttpClientHandler()
            {
                //Proxy = new WebProxy("localhost", 8888)
            });
            var subscriptionId = "8d2059f3-b805-41fa-ab84-e13d4dfec042";
            httpClient.BaseAddress = new Uri($"https://management.azure.com/");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer"
                , token.Token);

            async Task<T?> GetAsync<T>(string uri, string apiVersion)
            {
                var apiVersionQueryString = (uri.Contains("?") ? "&" : "?") + $"api-version={apiVersion}";
                var resourceUri = $"{uri}{apiVersionQueryString}";
                if (responseCache.ContainsKey(resourceUri))
                {
                    return JsonConvert.DeserializeObject<T>(
                        responseCache[resourceUri],
                        new AzureResourceConverter((r, v) => GetAsync<JObject>(r.Id, v)));
                }

                var responseContent = await httpClient.GetStringAsync(resourceUri);
                responseCache[resourceUri] = responseContent;
                return JsonConvert.DeserializeObject<T>(responseContent,
                    new AzureResourceConverter((r, v) => GetAsync<JObject>(r.Id, v)));
            }

            var resourceGroup = "apimgmtpoc";
            resourceGroup = string.IsNullOrWhiteSpace(resourceGroup) ? "Defence-PoC" : resourceGroup;

            var directResources = await GetAsync<AzureList<AzureResource>>(
                $"/subscriptions/{subscriptionId}/resources?$filter=resourceGroup eq '{resourceGroup}'", "2020-10-01");

            var msGraph = @$"<mxGraphModel>
	<root>
		<mxCell id=""0"" />
		<mxCell id=""1"" parent=""0"" />
{string.Join(Environment.NewLine, directResources.Value.SelectMany((v, idx) => v.ToDrawIo(idx % 5, idx / 5)))}
	</root>
</mxGraphModel>";

            await File.WriteAllTextAsync(responseCacheFile, JsonConvert.SerializeObject(responseCache));
            await File.WriteAllTextAsync(Path.Combine(directoryName, "graph.drawio"), msGraph);
        }
    }
}