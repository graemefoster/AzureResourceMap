using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureDiagrams.Resources.Retrievers.Custom;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class App : AzureResource, ICanBeAccessedViaAHostName, ICanEgressViaAVnet
{
    private string? _dockerRepo;
    private string? _searchService;
    private RelationshipHelper _hostNameDiscoverer = default!;
    public string ServerFarmId { get; set; } = default!;
    public string? VirtualNetworkSubnetId { get; set; }
    public string Kind { get; set; } = default!;

    public override string Image => Kind switch
    {
        { } str when str.Contains("workflowapp") =>"img/lib/azure2/integration/Logic_Apps.svg",
        { } str when str.Contains("functionapp") =>"img/lib/azure2/compute/Function_Apps.svg", 
        _ => "img/lib/azure2/app_services/App_Services.svg"
    };

    public string? AppInsightsKey { get; set; }

    public string[] EnabledHostNames { get; set; } = default!;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return EnabledHostNames.Any(
            hn => string.Compare(hn, hostname, StringComparison.InvariantCultureIgnoreCase) == 0);
    }


    public override async Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        await base.Enrich(full, additionalResources);
        VirtualNetworkSubnetId = full["properties"]!["virtualNetworkSubnetId"]?.Value<string>();
        ServerFarmId = full["properties"]!.Value<string>("serverFarmId")!;

        var config = additionalResources[AppResourceRetriever.ConfigAppSettingsList];

        var siteProperties = full["properties"]!["siteProperties"]?["properties"]?
            .Select(
                x => new KeyValuePair<string, string?>(
                    x.Value<string>("name")!,
                    x.Value<String>("value")))
            .ToDictionary(x => x.Key, x => x.Value)!;

        LookForContainerLink(siteProperties);

        var connectionStrings = additionalResources[AppResourceRetriever.ConnectionStringSettingsList]?
            ["properties"]!.ToObject<Dictionary<string, JObject>>()?.Values
            .Select(x => x.Value<string>("value")).Where(x => x != null).Select(x => x!) ?? Array.Empty<string>();

        var appSettings = config?["properties"]!.ToObject<Dictionary<string, object>>() ??
                          new Dictionary<string, object>();
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

    public override IEnumerable<AzureResource> DiscoverNewNodes(List<AzureResource> azureResources)
    {
        if (VirtualNetworkSubnetId != null)
        {
            VNetIntegration = new VNetIntegration($"{Id}.vnetintegration", VirtualNetworkSubnetId, this)
            {
                Name = Name
            };
            yield return VNetIntegration;
        }
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        if (AppInsightsKey != null)
        {
            var appInsights = allResources.OfType<AppInsights>()
                .SingleOrDefault(x => x.InstrumentationKey == AppInsightsKey);
            if (appInsights != null) CreateFlowTo(appInsights, "apm", Plane.Diagnostics);
        }

        if (_dockerRepo != null)
        {
            this.CreateFlowToHostName(allResources, _dockerRepo, "container pull", Plane.Runtime);
        }

        if (_searchService != null)
        {
            this.CreateFlowToHostName(allResources, _searchService, "search api", Plane.Runtime);
        }

        _hostNameDiscoverer.BuildRelationships(this, allResources);

        if (VNetIntegration != null)
        {
            CreateFlowTo(VNetIntegration, Plane.All);
        }

        base.BuildRelationships(allResources);
    }

    public VNetIntegration? VNetIntegration { get; private set; }

    public AzureResource EgressResource()
    {
        if (VNetIntegration != null) return VNetIntegration;
        return this;
    }
}