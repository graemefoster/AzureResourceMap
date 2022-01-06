using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using DrawIo.Azure.Core.Diagrams;
using DrawIo.Azure.Core.Resources;
using DrawIo.Azure.Core.Resources.Retrievers;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;

namespace DrawIo.Azure.Core;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var resourceGroup = "ovenmedia"; // //"DiagramBuildUp"; // "function-outbound-calls";

        var directoryName = @".\AzureResourceManager\";

        var token = new AzureCliCredential().GetToken(
            new TokenRequestContext(new[] { "https://management.azure.com/" }));

        var httpClient = new HttpClient();
        var subscriptionId = "8d2059f3-b805-41fa-ab84-e13d4dfec042";
        httpClient.BaseAddress = new Uri("https://management.azure.com/");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer"
            , token.Token);

        var newTest = new ArmResourceRetriever(httpClient);
        var resources = (await newTest.Retrieve(subscriptionId, resourceGroup)).ToArray();
        await DrawDiagram(resources, directoryName, resourceGroup);
    }

    private static async Task DrawDiagram(AzureResource[] resources, string directoryName, string resourceGroup)
    {
        var additionalNodes = resources.SelectMany(x => x.DiscoverNewNodes());
        var allNodes = resources.Concat(additionalNodes).ToArray();

        //Discover hidden links that aren't obvious through the resource manager
        //For example, a NIC / private endpoint linked to a subnet
        foreach (var resource in allNodes) resource.BuildRelationships(resources);

        var graph = new GeometryGraph();

        var nodeBuilders = allNodes.ToDictionary(x => x, x => x.CreateNodeBuilder());
        var nodes = nodeBuilders.SelectMany(x => x.Value.CreateNodes(nodeBuilders)).ToArray();
        var nodesGroupedByResource = nodes.GroupBy(x => x.Item1, x => x.Item2);
        var nodesDictionary = nodesGroupedByResource.ToDictionary(x => x.Key, x => x.ToArray());
        var edges = nodeBuilders.Values.SelectMany(x => x.CreateEdges(nodesDictionary)).ToArray();

        nodesDictionary.SelectMany(x => x.Value).ForEach(n =>
        {
            if (n is Cluster)
            {
                if (n.ClusterParent == null)
                    graph.RootCluster.AddChild(n);
            }
            else
            {
                graph.Nodes.Add(n);
            }
        });
        edges.ForEach(graph.Edges.Add);

        var sb = new StringBuilder();

        var routingSettings = new EdgeRoutingSettings
        {
            UseObstacleRectangles = true,
            BendPenalty = 10,
            EdgeRoutingMode = EdgeRoutingMode.StraightLine
        };

        var settings = new SugiyamaLayoutSettings
        {
            PackingAspectRatio = 3,
            PackingMethod = PackingMethod.Compact,
            LayerSeparation = 25,
            EdgeRoutingSettings = routingSettings,
            NodeSeparation = 25,
            ClusterMargin = 50
        };

        LayoutHelpers.CalculateLayout(graph, settings, null);

        var msGraph = @$"<mxGraphModel>
	<root>
		<mxCell id=""0"" />
		<mxCell id=""1"" parent=""0"" />
{string.Join(Environment.NewLine, graph.GetFlattenedNodesAndClusters().Select(v => ((CustomUserData)v.UserData).Draw()))}
{string.Join(Environment.NewLine, graph.Edges.Select(v => ((CustomUserData)v.UserData).Draw()))}
{sb}
	</root>
</mxGraphModel>";

        await File.WriteAllTextAsync(Path.Combine(directoryName, $"{resourceGroup}.drawio"), msGraph);
        Console.WriteLine(msGraph);
    }
}