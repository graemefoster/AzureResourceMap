using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using AzureDiagrams.Resources.Retrievers.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources.Retrievers.Custom;

/// <summary>
/// Had hoped to fetch linked-services but you query them on a different api, need a different token, and that api might be on a private endpoint... So leaving for now :(
/// </summary>
public class SynapseRetriever : ResourceRetriever<Synapse>
{
    public const string LinkedServices = "linkedservices";
    private readonly TokenCredential _tokenCredential;

    public SynapseRetriever(JObject basicAzureResourceJObject, TokenCredential tokenCredential) : base(
        basicAzureResourceJObject,
        fetchFullResource: true, apiVersion: "2021-06-01",
        extensions: new IResourceExtension[]
            { new DiagnosticsExtensions(), new PrivateEndpointExtensions(), new ManagedIdentityExtension() })
    {
        _tokenCredential = tokenCredential;
    }

    protected override async Task<Dictionary<string, JObject?>> AdditionalResourcesCustom(
        BasicAzureResourceInfo basicInfo, Dictionary<string, JObject?> initialResources, JObject? fullResource)
    {
        var token = await _tokenCredential.GetTokenAsync(
            new TokenRequestContext(new[] { "https://dev.azuresynapse.net" }), CancellationToken.None);
        var devEndpoint = fullResource!["properties"]!["connectivityEndpoints"]!.Value<string>("dev")!;
        var client = new HttpClient(); //TODO - might be nice to pull this out so I'm not newing this up. Minor though.
        client.Timeout = TimeSpan.FromSeconds(5);
        var msg = new HttpRequestMessage(HttpMethod.Get,
            $"{devEndpoint}/linkedServices?api-version=2019-06-01-preview");
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        try
        {
            var response = await client.SendAsync(msg);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return new Dictionary<string, JObject?>()
                {
                    [LinkedServices] = JsonConvert.DeserializeObject<JObject>(responseContent)!
                };
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(
                $"\tFailed to fetch linked services from {devEndpoint}. Response {response.StatusCode}|{await response.Content.ReadAsStringAsync()}. If Synapse uses Private Endpoints you will need to run this from a location with access.");
            Console.ResetColor();
            return new Dictionary<string, JObject?>();
        }
        catch (TimeoutException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(
                $"\tFailed to fetch linked services from {devEndpoint}. Timed out. If Synapse uses Private Endpoints you will need to run this from a location with access.");
            Console.ResetColor();
            return new Dictionary<string, JObject?>();
        }
    }
}