using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Diagrams;
using DrawIo.Azure.Core.Resources.Retrievers;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class App : AzureResource, ICanBeAccessedViaAHostName, IUseManagedIdentities
{
    private VNetIntegration? _azureVNetIntegrationResource;
    private string? _dockerRepo;
    public string? ServerFarmId { get; set; }
    public string? VirtualNetworkSubnetId { get; set; }
    public string Kind { get; set; } = default!;
    public Identity? Identity { get; set; }

    public override string Image => Kind switch
    {
        "functionapp,workflowapp" => "img/lib/azure2/integration/Logic_Apps.svg",
        "functionapp" => "img/lib/azure2/iot/Function_Apps.svg",
        _ => "img/lib/azure2/app_services/App_Services.svg"
    };

    public (string storageName, string storageSuffix)[] ConnectedStorageAccounts { get; set; } = default!;

    public string? AppInsightsKey { get; set; }

    public string[] KeyVaultReferences { get; set; } = default!;

    public string[] EnabledHostNames { get; set; } = default!;

    public string[] HostNamesAccessedInAppSettings { get; set; } = default!;

    public (string server, string database)[] DatabaseConnections { get; set; } = default!;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return EnabledHostNames.Any(
            hn => string.Compare(hn, hostname, StringComparison.InvariantCultureIgnoreCase) == 0);
    }

    public bool DoYouUseThisUserAssignedClientId(string id)
    {
        return Identity?.UserAssignedIdentities?.Keys.Any(k =>
            string.Compare(k, id, StringComparison.InvariantCultureIgnoreCase) == 0) ?? false;
    }

    public void CreateManagedIdentityFlowBackToMe(UserAssignedManagedIdentity userAssignedManagedIdentity)
    {
        CreateFlowTo(userAssignedManagedIdentity, "AAD Identity", FlowEmphasis.LessImportant);
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
        var appSettings = config["properties"]!.ToObject<Dictionary<string, object>>()!;

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

        var potentialAppInsightsKey = appSettings.Keys.FirstOrDefault(x =>
            x.Contains("appinsights", StringComparison.InvariantCultureIgnoreCase) &&
            x.Contains("key", StringComparison.InvariantCultureIgnoreCase));
        if (potentialAppInsightsKey != null) AppInsightsKey = (string)appSettings[potentialAppInsightsKey];

        EnabledHostNames = full["properties"]!["enabledHostNames"]!.Values<string>().Select(x => x!).ToArray();

        var potentialConnectionStrings = appSettings.Values.Union(connectionStrings);

        ConnectedStorageAccounts = potentialConnectionStrings
            .OfType<string>()
            .Where(appSetting => appSetting.Contains("DefaultEndpointsProtocol") &&
                                 appSetting.Contains("AccountName"))
            .Select(x =>
            {
                var parts = x!.Split(';')
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x =>
                        new KeyValuePair<string, string>(x.Split('=')[0].ToLowerInvariant(),
                            x.Split('=')[1].ToLowerInvariant()))
                    .ToDictionary(x => x.Key, x => x.Value);

                return (parts["accountname"],
                    "." + (parts.ContainsKey("endpointsuffix") ? parts["endpointsuffix"] : "core.windows.net"));
            })
            .Distinct()
            .ToArray();

        DatabaseConnections = potentialConnectionStrings
            .OfType<string>()
            .Where(appSetting => appSetting.Contains("Data Source=") &&
                                 appSetting.Contains("Initial Catalog="))
            .Select(x =>
            {
                var csb = new DbConnectionStringBuilder();
                csb.ConnectionString = x;
                return ((string)csb["Data Source"], (string)csb["Initial Catalog"]);
            })
            .ToArray();

        var kvRegex = new Regex(@"^\@Microsoft\.KeyVault\(VaultName\=(.*?);");
        KeyVaultReferences = potentialConnectionStrings
            .OfType<string>()
            .Select(x => kvRegex.Match(x))
            .Where(x => x.Success)
            .Select(x => x.Groups[1].Captures[0].Value)
            .ToArray();

        HostNamesAccessedInAppSettings = appSettings
            .Values
            .OfType<string>()
            .Select(x =>
                {
                    if (Uri.TryCreate(x, UriKind.Absolute, out var uri)) return uri.Host;

                    return string.Empty;
                }
            )
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();

        if (appSettings.ContainsKey("AzureSearchName"))
            HostNamesAccessedInAppSettings = HostNamesAccessedInAppSettings
                .Concat(new[] { $"{(string)appSettings["AzureSearchName"]}.search.windows.net" }).ToArray();
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
            _azureVNetIntegrationResource =
                new VNetIntegration($"{Id}.vnetintegration", VirtualNetworkSubnetId);
            yield return _azureVNetIntegrationResource;
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

        foreach (var storageAccount in ConnectedStorageAccounts)
        {
            var storage = allResources.OfType<StorageAccount>()
                .SingleOrDefault(x => x.Name.ToLowerInvariant() == storageAccount.storageName);
            if (storage != null)
            {
                CreateFlowViaVNetIntegrationOrDirect(allResources, storage, "uses",
                    hns => hns.Any(hn =>
                        hn.StartsWith(storageAccount.storageName) && hn.EndsWith(storageAccount.storageSuffix)));
            }
        }

        foreach (var databaseConnection in DatabaseConnections)
        {
            //TODO check server name as-well
            var database = allResources.OfType<ManagedSqlDatabase>().SingleOrDefault(x =>
                string.Compare(x.Name, databaseConnection.database, StringComparison.InvariantCultureIgnoreCase) == 0);

            if (database != null)
            {
                CreateFlowViaVNetIntegrationOrDirect(allResources, database, "sql",
                    hns => hns.Any(hn => hn.StartsWith(database.Name)));
            }
        }

        foreach (var keyVaultReference in KeyVaultReferences)
        {
            //TODO KeyVault via private endpoint. Needs a generic way to look for a host that is accessed via private endpoints.

            //TODO check server name as-well
            var keyVault = allResources.OfType<KeyVault>().SingleOrDefault(x =>
                string.Compare(x.Name, keyVaultReference, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (keyVault != null)
            {
                CreateFlowViaVNetIntegrationOrDirect(allResources, keyVault, "secrets",
                    hns => hns.Any(hn => keyVault.CanIAccessYouOnThisHostName(hn)));
            }
        }

        if (_dockerRepo != null)
        {
            var acr = allResources.OfType<ACR>().SingleOrDefault(x => x.CanIAccessYouOnThisHostName(_dockerRepo));
            if (acr != null)
            {
                CreateFlowViaVNetIntegrationOrDirect(allResources, acr, "pulls",
                    hns => hns.Any(hn => acr.CanIAccessYouOnThisHostName(hn)));
            }
        }

        allResources.OfType<ICanBeAccessedViaAHostName>()
            .Where(x => HostNamesAccessedInAppSettings.Any(x.CanIAccessYouOnThisHostName))
            .ForEach(x =>
            {
                CreateFlowViaVNetIntegrationOrDirect(allResources, (AzureResource)x, "calls",
                    hns => hns.Any(x.CanIAccessYouOnThisHostName));
            });
    }

    private void CreateFlowViaVNetIntegrationOrDirect(
        IEnumerable<AzureResource> allResources,
        AzureResource connectTo,
        string flowName,
        Func<string[], bool>? nicHostNameCheck)
    {
        var nics = allResources.OfType<Nic>().Where(nic => nicHostNameCheck(nic.HostNames)).ToArray();

        if (_azureVNetIntegrationResource != null)
        {
            CreateFlowTo(_azureVNetIntegrationResource);

            if (nics.Any())
            {
                nics.ForEach(nic => _azureVNetIntegrationResource.CreateFlowTo(nic, flowName));
            }
            else
            {
                //Assume all traffic going via vnet integration for simplicity.
                _azureVNetIntegrationResource.CreateFlowTo(connectTo, flowName);
            }
        }
        else
        {
            //direct flow to the resource (no vnet integration)
            CreateFlowTo(connectTo, flowName);
        }
    }
}