using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using Azure.Core;
using Azure.Identity;
using AzureDiagrams;

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
        
        rootCommand.SetHandler(cmd =>
        {
            var isGithubAction = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTION"));
            if (isGithubAction && cmd.Token == null)
                throw new ArgumentException("To run in a Github action you must provide an access token");

            cmd.Output = isGithubAction ? "/github/workspace" : cmd.Output;
            DrawDiagram(
                    Guid.Parse(cmd.SubscriptionId),
                    cmd.TenantId,
                    cmd.ResourceGroup,
                    cmd.Output,
                    cmd.Condensed,
                    cmd.ShowDiagnostics,
                    cmd.ShowInferred,
                    cmd.ShowRuntime,
                    cmd.ShowIdentity,
                    cmd.Token,
                    cmd.OutputFileName,
                    cmd.OutputPng)
                .Wait();
            
        }, new DiagramGeneratorCommandBinder(subscriptionIdOption, tenantIdOption, resourceGroupsOption, outputOption, condensedOption, showDiagnosticsOption, showInferredOption, showRuntimeOption, showIdentityOption, tokenOption, outputFileNameOption, outputPngOption));

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
            Console.WriteLine($"Tenant: {tenantId}");
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
                    ? (TokenCredential)new AzureCliCredential(new AzureCliCredentialOptions()
                    {
                        TenantId = tenantId
                    })
                    : new KnownTokenCredential(token);

            var cancellationTokenSource = new CancellationTokenSource();
            var azureResources = await new AzureModelRetriever().Retrieve(
                tokenCredential,
                cancellationTokenSource.Token,
                subscriptionId,
                tenantId,
                resourceGroups);

            var graph = await DrawIoDiagramGenerator.DrawDiagram(
                azureResources,
                condensed,
                showDiagnosticsFlows,
                showInferredFlows,
                showRuntimeFlows,
                showIdentityFlows);

            var outputName = string.IsNullOrWhiteSpace(outputFileName)
                ? $"{resourceGroups[0].Replace("*", "")}.drawio"
                : outputFileName;
            var fullOutputPath = Path.GetFullPath(Path.Combine(outputFolder, outputName));

            await File.WriteAllTextAsync(fullOutputPath, graph, cancellationTokenSource.Token);
            if (outputPng)
            {
                var pngOutput = $"{fullOutputPath}.png";
                var psi = new ProcessStartInfo("/drawio/drawio-x86_64-17.4.2.AppImage")
                {
                    Arguments =
                        $"--appimage-extract-and-run --export --format png --embed-diagram --output \"{pngOutput}\" \"{fullOutputPath}\" --no-sandbox",
                    UseShellExecute = false
                };
                Console.WriteLine($"/drawio/drawio-x86_64-17.4.2.AppImage {psi.Arguments}");
                var process = Process.Start(psi)!;
                await process.WaitForExitAsync(cancellationTokenSource.Token);
                Console.WriteLine($"Written PNG output to {pngOutput}");
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Written Draw.IO output to {fullOutputPath}");
            Console.ResetColor();
        }
        finally
        {
            Console.ResetColor();
        }
    }
}

public class DiagramGeneratorCommand
{
    public string SubscriptionId { get; init; }
    public string? TenantId { get; init; }
    public string[] ResourceGroup { get; init; }
    public string Output { get; set; }
    public bool Condensed { get; init; }
    public bool ShowDiagnostics { get; init; }
    public bool ShowInferred { get; init; }
    public bool ShowRuntime { get; init; }
    public bool ShowIdentity { get; init; }
    public string? Token { get; init; }
    public string? OutputFileName { get; init; }
    public bool OutputPng { get; init; }
}

public class DiagramGeneratorCommandBinder : BinderBase<DiagramGeneratorCommand>
{
    private readonly Option<string> _subscriptionIdOption;
    private readonly Option<string> _tenantIdOption;
    private readonly Option<string[]> _resourceGroupsOption;
    private readonly Option<string> _outputOption;
    private readonly Option<bool> _condensedOption;
    private readonly Option<bool> _showDiagnosticsOption;
    private readonly Option<bool> _showInferredOption;
    private readonly Option<bool> _showRuntimeOption;
    private readonly Option<bool> _showIdentityOption;
    private readonly Option<string> _tokenOption;
    private readonly Option<string> _outputFileNameOption;
    private readonly Option<bool> _outputPngOption;

    public DiagramGeneratorCommandBinder(Option<string> subscriptionIdOption, Option<string> tenantIdOption,
        Option<string[]> resourceGroupsOption, Option<string> outputOption, Option<bool> condensedOption,
        Option<bool> showDiagnosticsOption, Option<bool> showInferredOption, Option<bool> showRuntimeOption,
        Option<bool> showIdentityOption, Option<string> tokenOption, Option<string> outputFileNameOption,
        Option<bool> outputPngOption)
    {
        _subscriptionIdOption = subscriptionIdOption;
        _tenantIdOption = tenantIdOption;
        _resourceGroupsOption = resourceGroupsOption;
        _outputOption = outputOption;
        _condensedOption = condensedOption;
        _showDiagnosticsOption = showDiagnosticsOption;
        _showInferredOption = showInferredOption;
        _showRuntimeOption = showRuntimeOption;
        _showIdentityOption = showIdentityOption;
        _tokenOption = tokenOption;
        _outputFileNameOption = outputFileNameOption;
        _outputPngOption = outputPngOption;
    }
    protected override DiagramGeneratorCommand GetBoundValue(BindingContext bindingContext) =>
        new DiagramGeneratorCommand
        {
            SubscriptionId = bindingContext.ParseResult.GetValueForOption(_subscriptionIdOption)!,
            TenantId = bindingContext.ParseResult.GetValueForOption(_tenantIdOption),
            Condensed = bindingContext.ParseResult.GetValueForOption(_condensedOption),
            Output = bindingContext.ParseResult.GetValueForOption(_outputOption)!,
            OutputPng = bindingContext.ParseResult.GetValueForOption(_outputPngOption),
            Token = bindingContext.ParseResult.GetValueForOption(_tokenOption),
            ResourceGroup = bindingContext.ParseResult.GetValueForOption(_resourceGroupsOption)!,
            ShowDiagnostics = bindingContext.ParseResult.GetValueForOption(_showDiagnosticsOption),
            ShowIdentity = bindingContext.ParseResult.GetValueForOption(_showIdentityOption),
            ShowInferred = bindingContext.ParseResult.GetValueForOption(_showInferredOption),
            ShowRuntime = bindingContext.ParseResult.GetValueForOption(_showRuntimeOption),
            OutputFileName = bindingContext.ParseResult.GetValueForOption(_outputFileNameOption),
        };
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