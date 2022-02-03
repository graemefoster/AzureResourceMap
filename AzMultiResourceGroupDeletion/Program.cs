// See https://aka.ms/new-console-template for more information


using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Azure.Identity;
using DrawIo.Azure.Core;
using DrawIo.Azure.Core.Resources;

var subscriptionIdOption = new Option<string>("--subscription-id") { IsRequired = true };
var resourceGroupsOption = new Option<string[]>("--resource-group")
    { IsRequired = true, AllowMultipleArgumentsPerToken = true };
var outputOption = new Option<string>("--output") { IsRequired = true };
var rootCommand = new RootCommand("AzureDiagrams")
{
    subscriptionIdOption,
    resourceGroupsOption,
    outputOption
};
rootCommand.Handler = CommandHandler.Create((string subscriptionId, string[] resourceGroup, string output) =>
{
    SafeDelete(Guid.Parse(subscriptionId), resourceGroup, output).Wait();
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

static async Task SafeDelete(Guid subscriptionId, string[] resourceGroups, string outputFolder)
{
    try
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Subscription: {subscriptionId}");
        Console.WriteLine($"Resource Groups: {string.Join(',', resourceGroups)}");
        Console.ResetColor();

        var tokenCredential = new DefaultAzureCredential();
        var cancellationTokenSource = new CancellationTokenSource();
        var azureResources = await new AzureModelRetriever().Retrieve(
            tokenCredential,
            cancellationTokenSource.Token,
            subscriptionId, resourceGroups);

        var crossResourceGroupDependencyIssues = SequenceResourceDeletion(azureResources);
    }
    finally
    {
        Console.ResetColor();
    }
}

static AzureResource[] SequenceResourceDeletion(AzureResource[] azureResources)
{
    var list = new List<AzureResource>();
    var remaining = new List<AzureResource>(azureResources);

    var childResources = azureResources
        .OrderByDescending(x => x.Id.Split('/').Length)
        .Where(x => x.NestedDepth > 0)
        .ToArray();

    childResources.ForEach(x => remaining.Remove(x));
    list.AddRange(childResources);

    var managers = remaining.Where(x => x.ManagedBy != null).Select(x =>
            (x, remaining.SingleOrDefault(y => string.Equals(y.Id, x.Id, StringComparison.InvariantCultureIgnoreCase))))
        .ToArray();

    Console.ForegroundColor = ConsoleColor.Red;
    foreach (var pair in managers.Where(x => x.Item2 == null))
    {
        Console.WriteLine(
            $"Resource {pair.x.Id} is managed by {pair.x.ManagedBy}. This resource was not found so it will not be possible to delete these resource groups fully");
    }

    Console.ResetColor();

    foreach (var manager in managers.Where(x => x.Item2 != null))
    {
        remaining.Remove(manager.Item2!);
        list.Add(manager.Item2!);
    }

    return list.ToArray();
}