using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
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
    public static async Task<int> Main(string[] args)
    {
        var subscriptionIdOption = new Option<string>( "--subscription-id") { IsRequired = true };
        var resourceGroupsOption = new Option<string[]>("--resource-group") { IsRequired = true, AllowMultipleArgumentsPerToken = true};
        var outputOption = new Option<string>("--output") { IsRequired = true};
        var rootCommand = new RootCommand("AzureDiagrams")
        {
            subscriptionIdOption,
            resourceGroupsOption,
            outputOption
        };
        rootCommand.Handler = CommandHandler.Create((string subscriptionId, string[] resourceGroup, string output) =>
        {
            DrawDiagram(Guid.Parse(subscriptionId), resourceGroup, output).Wait();
        });

        var parser =
            new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .UseHelp()
                .UseExceptionHandler((e, ctx) =>
                {
                    Console.WriteLine(e.InnerException?.Message ?? e.Message);
                    Console.WriteLine(e.ToString());
                    ctx.ExitCode = -1;
                }).Build();

        return await parser.InvokeAsync(args);
    }

    private static async Task DrawDiagram(Guid subscriptionId, string[] resourceGroups, string outputFolder)
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Subscription: {subscriptionId}");
            Console.WriteLine($"Resource Groups: {string.Join(',', resourceGroups)}");
            Console.ResetColor();

            var token = new AzureCliCredential().GetToken(
                new TokenRequestContext(new[] { "https://management.azure.com/" }));

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://management.azure.com/");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            var armClient = new ArmClient(httpClient);
            var resources = (await armClient.Retrieve(subscriptionId, resourceGroups)).ToArray();
            await DrawDiagram(resources, outputFolder, resourceGroups[0]);
        }
        finally
        {
            Console.ResetColor();
        }
    }


    private static async Task DrawDiagram(AzureResource[] resources, string directoryName, string outputName)
    {
        var additionalNodes = resources.SelectMany(x => x.DiscoverNewNodes());

        //create some common nodes to represent common platform groupings (AAD, Diagnostics)
        var aad = new AzureActiveDirectory { Id = CommonResources.AAD, Name = "Azure Active Directory" };
        var core = new CoreServices { Id = CommonResources.CoreServices, Name = "Core Services" };
        var diagnostics = new CommonDiagnostics { Id = CommonResources.Diagnostics, Name = "Diagnostics" };
        var allNodes = resources.Concat(additionalNodes).Concat(new AzureResource[] { aad, diagnostics, core })
            .ToArray();

        //Discover hidden links that aren't obvious through the resource manager
        //For example, a NIC / private endpoint linked to a subnet
        foreach (var resource in allNodes) resource.BuildRelationships(allNodes);

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
            ClusterMargin = 50,
            
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

        var path = Path.Combine(directoryName, $"{outputName}.drawio");
        await File.WriteAllTextAsync(path, msGraph);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Written output to {Path.GetFullPath(path)}");
        Console.ResetColor();
    }
}