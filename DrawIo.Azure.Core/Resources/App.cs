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

public class App : AzureResource, ICanBeExposedByPrivateEndpoints, ICanBeAccessedViaHttp, IUseManagedIdentities
{
    private VNetIntegration? _azureVNetIntegrationResource;
    public string? ServerFarmId { get; set; }
    public string[] PrivateEndpoints { get; set; }
    public string? VirtualNetworkSubnetId { get; set; }
    public string Kind { get; set; }
    public Identity? Identity { get; set; }

    public override string Image => Kind switch
    {
        "functionapp,workflowapp" => "img/lib/azure2/integration/Logic_Apps.svg",
        "functionapp" => "img/lib/azure2/iot/Function_Apps.svg",
        _ => "img/lib/azure2/app_services/App_Services.svg"
    };

    public (string storageName, string storageSuffix)[] ConnectedStorageAccounts { get; set; }

    public string? AppInsightsKey { get; set; }

    public string[] KeyVaultReferences { get; set; } = default!;

    public string[] EnabledHostNames { get; set; } = default!;

    public string[] HostNamesAccessedInAppSettings { get; set; } = default!;

    public (string server, string database)[] DatabaseConnections { get; set; } = default!;

    public bool AccessedViaPrivateEndpoint(PrivateEndpoint privateEndpoint)
    {
        return PrivateEndpoints.Contains(privateEndpoint.Id.ToLowerInvariant());
    }

    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new AzureResourceNodeBuilder(this);
    }

    public override async Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        VirtualNetworkSubnetId = full["properties"]!["virtualNetworkSubnetId"]?.Value<string>();
        ServerFarmId = full["properties"]!["serverFarmId"]?.Value<string>();

        PrivateEndpoints =
            full["properties"]!["privateEndpointConnections"]?
                .Select(x => x["properties"]["privateEndpoint"].Value<string>("id").ToLowerInvariant())
                .ToArray();


        var config = additionalResources[AppResourceRetriever.ConfigAppSettingsList];
        var appSettings = config["properties"]!.ToObject<Dictionary<string, object>>()!;

        var potentialAppInsightsKey = appSettings.Keys.FirstOrDefault(x =>
            x.Contains("appinsights", StringComparison.InvariantCultureIgnoreCase) &&
            x.Contains("key", StringComparison.InvariantCultureIgnoreCase));
        if (potentialAppInsightsKey != null)
        {
            AppInsightsKey = (string)appSettings[potentialAppInsightsKey];
        }

        EnabledHostNames = full["properties"]!["enabledHostNames"]!.Values<string>().ToArray();

        ConnectedStorageAccounts = appSettings
            .Values
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

                return (parts["accountname"], "." + (parts.ContainsKey("endpointsuffix") ? parts["endpointsuffix"] : "core.windows.net"));
            })
            .Distinct()
            .ToArray();

        DatabaseConnections = appSettings
            .Values
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
        KeyVaultReferences = appSettings
            .Values
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
                    if (Uri.TryCreate(x, UriKind.Absolute, out var uri))
                    {
                        return uri.Host;
                    }

                    return string.Empty;
                }
            )
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();

        if (appSettings.ContainsKey("AzureSearchName"))
        {
            HostNamesAccessedInAppSettings = HostNamesAccessedInAppSettings
                .Concat(new[] { $"{(string)appSettings["AzureSearchName"]}.search.windows.net" }).ToArray();
        }
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
            if (appInsights != null) CreateFlowTo(appInsights);
        }
        
        foreach (var storageAccount in ConnectedStorageAccounts)
        {
            var storage = allResources.OfType<StorageAccount>()
                .SingleOrDefault(x => x.Name.ToLowerInvariant() == storageAccount.storageName);

            if (storage != null)
            {

                var flowSource = _azureVNetIntegrationResource as AzureResource ?? this;

                var nics = allResources.OfType<Nic>().Where(nic => nic.HostNames.Any(hn =>
                    hn.StartsWith(storageAccount.storageName) && hn.EndsWith(storageAccount.storageSuffix)));

                nics.ForEach(nic => flowSource.CreateFlowTo(nic, "Uses"));

                if (!nics.Any())
                {
                    flowSource.CreateFlowTo(storage, "Uses");
                }
                else
                {
                    //Had some NICs so let's use the vnet integration resource.
                    if (_azureVNetIntegrationResource != null) CreateFlowTo(_azureVNetIntegrationResource);
                }
            }
        }

        foreach (var databaseConnection in DatabaseConnections)
        {
            //TODO check server name as-well
            var database = allResources.OfType<ManagedSqlDatabase>().SingleOrDefault(x =>
                string.Compare(x.Name, databaseConnection.database, StringComparison.InvariantCultureIgnoreCase) == 0);
            
            if (database != null) CreateFlowTo(database, "SQL");
        }

        foreach (var keyVaultReference in KeyVaultReferences)
        {
            //TODO KeyVault via private endpoint. Needs a generic way to look for a host that is accessed via private endpoints.
            
            //TODO check server name as-well
            var keyVault = allResources.OfType<KeyVault>().SingleOrDefault(x =>
                string.Compare(x.Name, keyVaultReference, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (keyVault != null) CreateFlowTo(keyVault, "Secrets");
        }

        allResources.OfType<ICanBeAccessedViaHttp>()
            .Where(x => HostNamesAccessedInAppSettings.Any(x.CanIAccessYouOnThisHostName))
            .ForEach(x => CreateFlowTo((AzureResource)x, "Calls"));
    }

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

    public void CreateFlowBackToMe(UserAssignedManagedIdentity userAssignedManagedIdentity)
    {
        CreateFlowTo(userAssignedManagedIdentity, "AAD Identity");
    }
}