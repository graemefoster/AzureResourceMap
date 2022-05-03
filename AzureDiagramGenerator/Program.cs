using System.Collections.ObjectModel;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
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
        var subscriptionIdOption = new Option<string>("--subscription-id") { IsRequired = true };
        var tenantIdOption = new Option<string>("--tenant-id") { IsRequired = false };
        var resourceGroupsOption = new Option<string[]>("--resource-group")
        {
            IsRequired = true, AllowMultipleArgumentsPerToken = true,
            Description =
                "Resource group(s) containing resources. You can pass multiple, and used the wildcard * character."
        };
        var outputOption = new Option<string>("--output")
            { IsRequired = false, Description = "Output folder for generated diagram" };

        var condensedOption = new Option<bool>("--condensed")
        {
            IsRequired = false,
            Description =
                "Condenses Private Endpoints / VNet Integration. For large deployments this can greatly simplify the diagram."
        };

        var showDiagnosticsOption = new Option<bool>("--show-diagnostics")
        {
            IsRequired = false, Description = "Show diagnostics flows"
        };

        var showInferredOption = new Option<bool>("--show-inferred")
        {
            IsRequired = false, Description = "Show runtime flows inferred from app-settings"
        };

        var showRuntimeOption = new Option<bool>("--show-runtime")
        {
            IsRequired = false, Description = "Show runtime flows defined on the management plane"
        };

        var showIdentityOption = new Option<bool>("--show-identity")
        {
            IsRequired = false, Description = "Show runtime flows to managed identities"
        };

        var tokenOption = new Option<string>("--token")
            { IsRequired = false, Description = "Access token that can read the resources" };

        var outputFileNameOption = new Option<string>("--output-file-name")
        {
            IsRequired = false,
            Description =
                "File name to write to (will use convention to determine based on a resource-group name if not provided)"
        };

        var outputPngOption = new Option<bool>("--output-png")
            { IsRequired = false, Description = "Output png file with draw.io diagram embedded" };

        var rootCommand = new RootCommand("AzureDiagrams")
        {
            subscriptionIdOption,
            tenantIdOption,
            resourceGroupsOption,
            outputOption,
            condensedOption,
            showDiagnosticsOption,
            showInferredOption,
            showRuntimeOption,
            showIdentityOption,
            tokenOption,
            outputFileNameOption,
            outputPngOption
        };
        rootCommand.Handler =
            CommandHandler.Create(
                (string subscriptionId, string? tenantId, string[] resourceGroup, string output, bool condensed,
                    bool showDiagnostics, bool showInferred, bool showRuntime, bool showIdentity, string? token,
                    string? outputFileName, bool outputPng) =>
                {
                    var isGithubAction = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTION"));
                    if (isGithubAction && token == null)
                        throw new ArgumentException("To run in a Github action you must provide an access token");

                    output = isGithubAction ? "/github/workspace" : output;
                    DrawDiagram(
                            Guid.Parse(subscriptionId),
                            tenantId,
                            resourceGroup,
                            output,
                            condensed,
                            showDiagnostics,
                            showInferred,
                            showRuntime,
                            showIdentity,
                            token,
                            outputFileName,
                            outputPng)
                        .Wait();
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

    private static async Task DrawDiagram(
        Guid subscriptionId,
        string? tenantId,
        string[] resourceGroups,
        string outputFolder,
        bool condensed,
        bool showDiagnosticsFlows,
        bool showInferredFlows,
        bool showRuntimeFlows,
        bool showIdentityFlows,
        string? token,
        string? outputFileName,
        bool outputPng)
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Subscription: {subscriptionId}");
            Console.WriteLine($"Resource Groups: {string.Join(',', resourceGroups)}");

            if (!string.IsNullOrEmpty(outputFolder))
            {
                Console.WriteLine($"Output Folder: {outputFolder}");
            }

            if (!string.IsNullOrEmpty(outputFileName))
            {
                Console.WriteLine($"Output File: {outputFileName}");
            }

            Console.WriteLine($"Condensed: {condensed}");
            Console.WriteLine($"Show Identity Flows: {showIdentityFlows}");
            Console.WriteLine($"Show Diagnostics Flows: {showDiagnosticsFlows}");
            Console.WriteLine($"Show Inferred Flows: {showInferredFlows}");
            Console.WriteLine($"Show Runtime Flows: {showRuntimeFlows}");
            Console.ResetColor();

            var tokenCredential =
                string.IsNullOrWhiteSpace(token)
                    ? (TokenCredential)new AzureCliCredential()
                    : new KnownTokenCredential(token);

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
                showDiagnosticsFlows,
                showInferredFlows,
                showRuntimeFlows,
                showIdentityFlows);

            var outputName = string.IsNullOrWhiteSpace(outputFileName)
                ? $"{resourceGroups[0].Replace("*", "")}.drawio"
                : outputFileName;
            var path = Path.Combine(outputFolder, outputName);

            if (outputPng)
            {
                await File.WriteAllTextAsync(path, graph, cancellationTokenSource.Token);
                var psi = new ProcessStartInfo("/drawio/drawio-x86_64-17.4.2.AppImage")
                {
                    Arguments =
                        $"--appimage-extract-and-run --export --format png --embed-diagram \"{path}\" --no-sandbox",
                    UseShellExecute = false
                };
                Console.WriteLine($"/drawio/drawio-x86_64-17.4.2.AppImage {psi.Arguments}");
                var process = Process.Start(psi)!;
                await process.WaitForExitAsync(cancellationTokenSource.Token);
                Console.WriteLine($"Written output to {Path.GetFullPath(path)}.png");
            }
            else
            {
                await File.WriteAllTextAsync(path, graph, cancellationTokenSource.Token);
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Written output to {Path.GetFullPath(path)}");
            Console.ResetColor();
        }
        finally
        {
            Console.ResetColor();
        }
    }


    private static Task<string> DrawDiagram(
        AzureResource[] resources,
        bool condensed,
        bool showDiagnosticsFlows,
        bool showInferredFlows,
        bool showRuntimeFlows,
        bool showIdentityFlows
    )
    {
        var graph = new GeometryGraph();

        var planes = showDiagnosticsFlows ? Plane.Diagnostics : Plane.None;
        planes |= showInferredFlows ? Plane.Inferred : Plane.None;
        planes |= showRuntimeFlows ? Plane.Runtime : Plane.None;
        planes |= showIdentityFlows ? Plane.Identity : Plane.None;
        var adjustor = (IDiagramAdjustor)new VisiblePlanesDiagramAdjustor(planes);
        adjustor = condensed ? new CondensedDiagramAdjustor(adjustor, resources) : adjustor;

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
            Padding = 5,
            BendPenalty = 10,
            UseObstacleRectangles = true,
            EdgeRoutingMode = EdgeRoutingMode.Rectilinear
        };

        var settings = new SugiyamaLayoutSettings
        {
            PackingAspectRatio = 3,
            PackingMethod = PackingMethod.Compact,
            LayerSeparation = 25,
            EdgeRoutingSettings = routingSettings,
            LiftCrossEdges = true,
            NodeSeparation = 25,
            ClusterMargin = 50,
        };

        LayoutHelpers.CalculateLayout(graph, settings, null);

        var msGraph = @$"<mxGraphModel>
	<root>
		<mxCell id=""0"" />
		<mxCell id=""1"" parent=""0"" />
{string.Join(Environment.NewLine, graph.GetFlattenedNodesAndClusters().Select(v => ((CustomUserData)v.UserData).DrawNode!()))}
{string.Join(Environment.NewLine, graph.Edges.Select(v => ((CustomUserData)v.UserData).DrawEdge!(v)))}
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

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext,
        CancellationToken cancellationToken)
    {
        return _token;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return _token.Result;
    }
}