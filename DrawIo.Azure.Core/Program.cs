using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using DrawIo.Azure.Core.Resources;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var resourceGroup = "DiagramBuildUp"; // "function-outbound-calls";

        var directoryName = @".\AzureResourceManager\";

        var responseCacheFile =
            Path.Combine(
                directoryName,
                $"responseCache.{resourceGroup}.json");

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(responseCacheFile)));

        var responseCache = File.Exists(responseCacheFile)
            ? JsonConvert.DeserializeObject<Dictionary<string, string>>(
                await File.ReadAllTextAsync(responseCacheFile))!
            : new Dictionary<string, string>();


        var token = new AzureCliCredential().GetToken(
            new TokenRequestContext(new[] { "https://management.azure.com/" }));

        var httpClient = new HttpClient();
        var subscriptionId = "8d2059f3-b805-41fa-ab84-e13d4dfec042";
        httpClient.BaseAddress = new Uri($"https://management.azure.com/");
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer"
            , token.Token);

        async Task<T?> GetAsync<T>(string uri, string apiVersion, HttpMethod? method = null)
        {
            var apiVersionQueryString = (uri.Contains("?") ? "&" : "?") + $"api-version={apiVersion}";
            var resourceUri = $"{uri}{apiVersionQueryString}";
            if (responseCache.ContainsKey(resourceUri))
            {
                return JsonConvert.DeserializeObject<T>(
                    responseCache[resourceUri],
                    new AzureResourceConverter(
                        (r, v) => GetAsync<JObject>(r.Id, v)!,
                        r =>
                            r
                                .AdditionalResources
                                .ToDictionary(
                                    x => x.Item2,
                                    x => GetAsync<JObject>($"{r.Id}/{x.Item2}", r.ApiVersion, x.Item1))!));
            }

            var request = new HttpRequestMessage(method ?? HttpMethod.Get, resourceUri);
            var responseContent = await (await httpClient.SendAsync(request)).Content.ReadAsStringAsync();
            responseCache[resourceUri] = responseContent;
            return JsonConvert.DeserializeObject<T>(responseContent,
                new AzureResourceConverter(
                    (r, v) => GetAsync<JObject>(r.Id, v)!,
                    r =>
                        r
                            .AdditionalResources
                            .ToDictionary(
                                x => x.Item2,
                                x => GetAsync<JObject>($"{r.Id}/{x.Item2}", r.ApiVersion, x.Item1))!));
        }


        var directResources = await GetAsync<AzureList<AzureResource>>(
            $"/subscriptions/{subscriptionId}/resources?$filter=resourceGroup eq '{resourceGroup}'", "2020-10-01");

        await DrawDiagram(directResources, responseCacheFile, responseCache, directoryName, resourceGroup);
    }

    private static async Task DrawDiagram(AzureList<AzureResource>? directResources, string responseCacheFile,
        Dictionary<string, string> responseCache,
        string directoryName, string resourceGroup)
    {
        var graph = new GeometryGraph();
        foreach (var resource in directResources!.Value)
        {
            var nodes = resource.CreateNodeBuilder().CreateNodes(resource);
            foreach (var node in nodes)
            {
                graph.Nodes.Add(node);
            }
        }

        var sb = new StringBuilder();

        //Discover hidden links that aren't obvious through the resource manager
        //For example, a NIC / private endpoint linked to a subnet
        // foreach (var resource in directResources.Value)
        // {
        //     resource.Link(directResources.Value, graph);
        // }

        //Group items into clusters - for example a subnet contains many vms, or an app-service-plan contains many apps
        // foreach (var resource in directResources.Value.OfType<IContainResources>())
        // {
        //     resource.CreateNodes(graph, directResources.Value);
        // }

        var routingSettings = new EdgeRoutingSettings
        {
            UseObstacleRectangles = true,
            BendPenalty = 10,
            EdgeRoutingMode = EdgeRoutingMode.StraightLine
        };

        var settings = new SugiyamaLayoutSettings()
        {
            ClusterMargin = 5,
            PackingAspectRatio = 3,
            PackingMethod = PackingMethod.Compact,
            RepetitionCoefficientForOrdering = 0,
            LayerSeparation = 50,
            EdgeRoutingSettings = routingSettings,
            NodeSeparation = 50
        };

        LayoutHelpers.CalculateLayout(graph, settings, null);

        var msGraph = @$"<mxGraphModel>
	<root>
		<mxCell id=""0"" />
		<mxCell id=""1"" parent=""0"" />
{string.Join(Environment.NewLine, graph.Nodes.Select(v => ((CustomUserData)v.UserData).Draw()))}
{sb}
	</root>
</mxGraphModel>";

        await File.WriteAllTextAsync(responseCacheFile, JsonConvert.SerializeObject(responseCache));
        await File.WriteAllTextAsync(Path.Combine(directoryName, $"{resourceGroup}.drawio"), msGraph);
        Console.WriteLine(msGraph);
    }
}