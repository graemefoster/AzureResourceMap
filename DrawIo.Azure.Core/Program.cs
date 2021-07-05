using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using DrawIo.Azure.Core.Resources;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Node = Microsoft.Msagl.Core.Layout.Node;

namespace DrawIo.Azure.Core
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var resourceGroup = "apimgmtpoc";// "function-outbound-calls";
            resourceGroup = string.IsNullOrWhiteSpace(resourceGroup) ? "Defence-PoC" : resourceGroup;

            var directoryName = @"C:\Users\graemefoster\Documents\LINQPad Queries\AzureResourceManager\";

            var responseCacheFile =
                Path.Combine(
                    directoryName,
                    $"responseCache.{resourceGroup}.json");

            var responseCache = File.Exists(responseCacheFile)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(await File.ReadAllTextAsync(responseCacheFile))!
                : new Dictionary<string, string>();


            var token = new AzureCliCredential().GetToken(
                new TokenRequestContext(new[] {"https://management.azure.com/"}));

            var httpClient = new HttpClient();
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


            var directResources = await GetAsync<AzureList<AzureResource>>(
                $"/subscriptions/{subscriptionId}/resources?$filter=resourceGroup eq '{resourceGroup}'", "2020-10-01");

            var graph = new GeometryGraph();
            foreach (var resource in directResources.Value)
            {
                var node = new Node(CurveFactory.CreateRectangle(50, 50, new Point(50, 25)))
                {
                    DebugId = resource
                };
                graph.Nodes.Add(node);
                resource.Node = node;
            }

            foreach (var resource in directResources.Value.OfType<IContainResources>())
            {
                resource.Group(graph, directResources.Value);
            }

            var sb = new StringBuilder();
            foreach (var resource in directResources.Value)
            {
                resource.Link(directResources.Value, graph);
            }
            foreach (var node in graph.Nodes)
            {
                node.BoundaryCurve = 
                    CurveFactory.CreateRectangle(150, 50, new Point(0, 0));
            }

            var routingSettings = new EdgeRoutingSettings {
                UseObstacleRectangles = true,
                BendPenalty = 100,
                EdgeRoutingMode = EdgeRoutingMode.StraightLine
            };
            
            var settings = new SugiyamaLayoutSettings {
                ClusterMargin = 50,
                PackingAspectRatio = 3,
                PackingMethod = PackingMethod.Columns,
                RepetitionCoefficientForOrdering = 0,
                EdgeRoutingSettings = routingSettings,
                NodeSeparation = 50,
                LayerSeparation = 150
            };
            
            LayoutHelpers.CalculateLayout(graph, settings, null);
            
            var msGraph = @$"<mxGraphModel>
	<root>
		<mxCell id=""0"" />
		<mxCell id=""1"" parent=""0"" />
{string.Join(Environment.NewLine, directResources.Value.SelectMany((v, idx) => v.ToDrawIo()))}
{sb}
	</root>
</mxGraphModel>";

            await File.WriteAllTextAsync(responseCacheFile, JsonConvert.SerializeObject(responseCache));
            await File.WriteAllTextAsync(Path.Combine(directoryName, "graph.drawio"), msGraph);
        }
    }
}