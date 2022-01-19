using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Diagrams;
using DrawIo.Azure.Core.Resources.Retrievers.Custom;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class App : AzureResource, ICanBeAccessedViaAHostName, ICanEgressViaAVnet
{
    private string? _dockerRepo;
    private string? _searchService;
    private RelationshipHelper _hostNameDiscoverer = default!;
    public string? ServerFarmId { get; set; }
    public string? VirtualNetworkSubnetId { get; set; }
    public string Kind { get; set; } = default!;

    public override string Image => Kind switch
    {
        "functionapp,workflowapp" => "img/lib/azure2/integration/Logic_Apps.svg",
        "functionapp" => "img/lib/azure2/iot/Function_Apps.svg",
        _ => "img/lib/azure2/app_services/App_Services.svg"
    };

    public string? AppInsightsKey { get; set; }

    public string[] EnabledHostNames { get; set; } = default!;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return EnabledHostNames.Any(
            hn => string.Compare(hn, hostname, StringComparison.InvariantCultureIgnoreCase) == 0);
    }

    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new AzureResourceNodeBuilder(this);
    }

    public override async Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        await base.Enrich(full, additionalResources);
        VirtualNetworkSubnetId = full["properties"]!["virtualNetworkSubnetId"]?.Value<string>();
        ServerFarmId = full["properties"]!["serverFarmId"]?.Value<string>();

        var config = additionalResources[AppResourceRetriever.ConfigAppSettingsList];

        var siteProperties = full["properties"]!["siteProperties"]?["properties"]?
            .Select(
                x => new KeyValuePair<string, string?>(
                    x.Value<string>("name")!,
                    x.Value<String>("value")))
            .ToDictionary(x => x.Key, x => x.Value)!;

        LookForContainerLink(siteProperties);

        var connectionStrings = additionalResources[AppResourceRetriever.ConnectionStringSettingsList]
            ["properties"]!.ToObject<Dictionary<string, JObject>>()?.Values
            .Select(x => x.Value<string>("value")).Where(x => x != null).Select(x => x!) ?? Array.Empty<string>();

        var appSettings = config["properties"]!.ToObject<Dictionary<string, object>>()!;
        var potentialConnectionStrings = appSettings.Values.Union(connectionStrings).ToArray();
        _hostNameDiscoverer = new RelationshipHelper(potentialConnectionStrings);
        _hostNameDiscoverer.Discover();

        var potentialAppInsightsKey = appSettings.Keys.FirstOrDefault(x =>
            x.Contains("appinsights", StringComparison.InvariantCultureIgnoreCase) &&
            x.Contains("key", StringComparison.InvariantCultureIgnoreCase));
        
        if (potentialAppInsightsKey != null) AppInsightsKey = (string)appSettings[potentialAppInsightsKey];

        EnabledHostNames = full["properties"]!["enabledHostNames"]!.Values<string>().Select(x => x!).ToArray();

        if (appSettings.ContainsKey("AzureSearchName"))
        {
            _searchService = $"{(string)appSettings["AzureSearchName"]}.search.windows.net";
        }
    }

    /// <summary>
    /// Look in site properties for anything starting with DOCKER| 
    /// </summary>
    /// <param name="siteProperties"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void LookForContainerLink(Dictionary<string, string?> siteProperties)
    {
        var regex = new Regex(@"^DOCKER[|](.*?)\/");
        _dockerRepo = siteProperties.Values.Where(x => x != null).Select(x => regex.Match(x!))
            .FirstOrDefault(x => x.Success)?.Groups[1].Captures[0]
            .Value;
    }

    public override IEnumerable<AzureResource> DiscoverNewNodes()
    {
        if (VirtualNetworkSubnetId != null)
        {
            VNetIntegration = new VNetIntegration($"{Id}.vnetintegration", VirtualNetworkSubnetId);
            yield return VNetIntegration;
        }
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        if (AppInsightsKey != null)
        {
            var appInsights = allResources.OfType<AppInsights>()
                .SingleOrDefault(x => x.InstrumentationKey == AppInsightsKey);
            if (appInsights != null) CreateFlowTo(appInsights, "apm", FlowEmphasis.LessImportant);
        }

        if (_dockerRepo != null)
        {
            this.CreateFlowToHostName(allResources, _dockerRepo, "pulls");
        }

        if (_searchService != null)
        {
            this.CreateFlowToHostName(allResources, _searchService, "uses");
        }

        _hostNameDiscoverer.BuildRelationships(this, allResources);

        base.BuildRelationships(allResources);
    }
    private VNetIntegration? VNetIntegration { get; set; }
    
    public AzureResource EgressResource()
    {
        if (VNetIntegration != null) return VNetIntegration;
        return this;
    }
}