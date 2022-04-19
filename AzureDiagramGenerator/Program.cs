using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Text;
using Azure.Core;
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
        var subscriptionIdOption = new Option<string>("--subscription-id") { IsRequired = true};
        var tenantIdOption = new Option<string>("--tenant-id") { IsRequired = false };
        var resourceGroupsOption = new Option<string[]>("--resource-group")
            { IsRequired = true, AllowMultipleArgumentsPerToken = true, Description = "Resource group(s) containing resources. You can pass multiple, and used the wildcard * character."};
        var outputOption = new Option<string>("--output") { IsRequired = false, Description = "Output folder for generated diagram" };
        var condensedOption = new Option<bool>("--condensed") { IsRequired = false, Description = "Condenses Private Endpoints / VNet Integration. For large deployments this can greatly simplify the diagram." };
        var noInferOption = new Option<bool>("--no-infer") { IsRequired = false, Description = "Do not attempt to infer relationships based on config settings" };
        var tokenOption = new Option<string>("--token") { IsRequired = false, Description = "Access token that can read the resources" };

        var rootCommand = new RootCommand("AzureDiagrams")
        {
            subscriptionIdOption,
            tenantIdOption,
            resourceGroupsOption,
            outputOption,
            condensedOption,
            noInferOption,
            tokenOption
        };
        rootCommand.Handler =
            CommandHandler.Create(
                (string subscriptionId, string? tenantId, string[] resourceGroup, string output, bool condensed, bool noInfer, string? token) =>
                {
                    var isGithubAction = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTION"));
                    DrawDiagram(Guid.Parse(subscriptionId), tenantId, resourceGroup, output, condensed, noInfer, isGithubAction, token).Wait();
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
        string outputFolder, bool condensed, bool noInfer, bool isGithubAction, string? token)
    {
        try
        {
            if (isGithubAction && token == null)
                throw new ArgumentException("To run in a Github action you must provide an access token");
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Is Github Action: {isGithubAction}");
            Console.WriteLine($"Subscription: {subscriptionId}");
            Console.WriteLine($"Resource Groups: {string.Join(',', resourceGroups)}");
            
            if (!string.IsNullOrEmpty(outputFolder))
            {
                Console.WriteLine($"Output Folder: {outputFolder}");
            }

            Console.WriteLine($"Condensed: {condensed}");
            Console.WriteLine($"NoInfer: {noInfer}");
            Console.ResetColor();

            var tokenCredential = token == null ? (TokenCredential)new AzureCliCredential() : new KnownTokenCredential(token);

            var cancellationTokenSource = new CancellationTokenSource();
            var azureResources = await new AzureModelRetriever().Retrieve(
                tokenCredential,
                cancellationTokenSource.Token,
                subscriptionId, 
                tenantId, 
                resourceGroups);

            var graph = await DrawDiagram(
                azureResources, 
                condensed,
                noInfer);

            if (!isGithubAction && !string.IsNullOrEmpty(outputFolder))
            {
                var outputName = resourceGroups[0].Replace("*", "");
                var path = Path.Combine(outputFolder, $"{outputName}.drawio");
                await File.WriteAllTextAsync(path, graph);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Written output to {Path.GetFullPath(path)}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"::set-output azurediagram={graph.ReplaceLineEndings(string.Empty)}");
            }
        }
        finally
        {
            Console.ResetColor();
        }
    }


    private static Task<string> DrawDiagram(
        AzureResource[] resources, 
        bool condensed, 
        bool noInfer)
    {
        var graph = new GeometryGraph();
        
        IDiagramAdjustor adjustor = condensed ? new CondensedDiagramAdjustor(resources, noInfer) : new NoOpDiagramAdjustor(noInfer);

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

        return Task.FromResult(msGraph);
    }
}

internal class KnownTokenCredential : TokenCredential
{
    private readonly ValueTask<AccessToken> _token;

    public KnownTokenCredential(string token)
    {
        _token = ValueTask.FromResult(new AccessToken(token, DateTimeOffset.MaxValue));
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return _token;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return _token.Result;
    }
}