﻿using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Text;
using Azure.Identity;
using AzureDiagramGenerator.DrawIo;
using AzureDiagramGenerator.DrawIo.DiagramAdjustors;
using AzureDiagrams;
using AzureDiagrams.Resources;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;

namespace AzureDiagramGenerator;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var subscriptionIdOption = new Option<string>("--subscription-id") { IsRequired = true };
        var tenantIdOption = new Option<string>("--tenant-id") { IsRequired = false };
        var resourceGroupsOption = new Option<string[]>("--resource-group")
            { IsRequired = true, AllowMultipleArgumentsPerToken = true };
        var outputOption = new Option<string>("--output") { IsRequired = true };
        var condensedOption = new Option<bool>("--condensed") { IsRequired = false };
        var rootCommand = new RootCommand("AzureDiagrams")
        {
            subscriptionIdOption,
            tenantIdOption,
            resourceGroupsOption,
            outputOption,
            condensedOption
        };
        rootCommand.Handler =
            CommandHandler.Create(
                (string subscriptionId, string? tenantId, string[] resourceGroup, string output, bool condensed) =>
                {
                    DrawDiagram(Guid.Parse(subscriptionId), tenantId, resourceGroup, output, condensed).Wait();
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

    private static async Task DrawDiagram(Guid subscriptionId, string? tenantId, string[] resourceGroups,
        string outputFolder, bool condensed)
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Subscription: {subscriptionId}");
            Console.WriteLine($"Resource Groups: {string.Join(',', resourceGroups)}");
            Console.WriteLine($"Output Folder: {outputFolder}");
            Console.WriteLine($"Condensed: {condensed}");
            Console.ResetColor();

            var tokenCredential = new AzureCliCredential();

            var cancellationTokenSource = new CancellationTokenSource();
            var azureResources = await new AzureModelRetriever().Retrieve(
                tokenCredential,
                cancellationTokenSource.Token,
                subscriptionId, tenantId, resourceGroups);

            await DrawDiagram(azureResources, outputFolder, resourceGroups[0].Replace("*", ""), condensed);
        }
        finally
        {
            Console.ResetColor();
        }
    }


    private static async Task DrawDiagram(AzureResource[] resources, string directoryName, string outputName,
        bool condensed)
    {
        var graph = new GeometryGraph();
        IDiagramAdjustor adjustor = condensed ? new CondensedDiagramAdjustor(resources) : new NoOpDiagramAdjustor();

        var nodeBuilders = resources.ToDictionary(x => x, x => AzureResourceNodeBuilder.CreateNodeBuilder(x, adjustor));
        var nodes = nodeBuilders.SelectMany(x => x.Value.CreateNodes(nodeBuilders, adjustor)).ToArray();
        var nodesGroupedByResource = nodes.GroupBy(x => x.Item1, x => x.Item2);
        var nodesDictionary = nodesGroupedByResource.ToDictionary(x => x.Key, x => x.ToArray());

        var edges = nodeBuilders.Values.SelectMany(x => x.CreateEdges(nodesDictionary, adjustor)).ToArray();

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