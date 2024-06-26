using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace AzureDiagrams.Resources;

public class RelationshipHelper
{
    private readonly string[] _potentialConnectionStrings;
    private static readonly Regex KvRegex = new Regex(@"^\@Microsoft\.KeyVault\(VaultName\=(.*?);");
    private static readonly Regex HostNameLikeRegex = new Regex(@"\/\/(([A-Za-z0-9-]{2,100}\.?)+)\b");
    private static readonly Regex SimpleHostNameLikeRegex = new Regex(@"^[A-Za-z0-9-_]{2,100}$");

    private (string storageName, string storageSuffix)[] _connectedStorageAccounts = default!;
    private (string serverName, string database)[] _databaseConnections = default!;
    private string[] _keyVaultReferences = default!;
    private string[] _hostNamesAccessedInAppSettings = default!;

    public RelationshipHelper(string[] potentialConnectionStrings)
    {
        _potentialConnectionStrings = potentialConnectionStrings;
    }

    public void Discover()
    {
        _connectedStorageAccounts = _potentialConnectionStrings
            .OfType<string>()
            .Where(appSetting => appSetting.Contains("DefaultEndpointsProtocol") &&
                                 appSetting.Contains("AccountName"))
            .Select(x =>
            {
                var parts = x!.Split(';')
                    .Where(part => !string.IsNullOrEmpty(part))
                    .Select(part => new KeyValuePair<string, string>(part.Split('=')[0].ToLowerInvariant(),
                        part.Split('=')[1].ToLowerInvariant()))
                    .ToDictionary(part => part.Key, part => part.Value);

                return (parts["accountname"],
                    "." + (parts.ContainsKey("endpointsuffix") ? parts["endpointsuffix"] : "core.windows.net"));
            })
            .Distinct()
            .ToArray();

        _databaseConnections = _potentialConnectionStrings
            .Where(appSetting => (appSetting.Contains("Data Source=") || appSetting.Contains("Server")) &&
                                 (appSetting.Contains("Initial Catalog=") || appSetting.Contains("Database=")))
            .Select(x =>
            {
                var csb = new DbConnectionStringBuilder
                {
                    ConnectionString = x
                };
                return
                    ((string)(csb.ContainsKey("Data Source") ? csb["Data Source"] : csb["Server"]),
                        (string)(csb.ContainsKey("Initial Catalog") ? csb["Initial Catalog"] : csb["Database"]));
            }).ToArray();

        _keyVaultReferences = _potentialConnectionStrings
            .Select(x => KvRegex.Match(x))
            .Where(x => x.Success)
            .Select(x => x.Groups[1].Captures[0].Value)
            .ToArray();

        _hostNamesAccessedInAppSettings = _potentialConnectionStrings
            .Select(x =>
                {
                    if (Uri.TryCreate(x, UriKind.Absolute, out var uri)) return uri.Host;

                    //try look for a URL like pattern in the string
                    var match = HostNameLikeRegex.Match(x);
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }

                    return string.Empty;
                }
            )
            .Where(x => !string.IsNullOrEmpty(x))
            .Union(_potentialConnectionStrings.Where(x => SimpleHostNameLikeRegex.IsMatch(x)))
            .Distinct()
            .ToArray();
    }

    /// <summary>
    /// Build flows
    /// </summary>
    /// <remarks>
    /// If you have a public service that has vnet integration (e.g. App in app-service-plan) then you may optionally access
    /// some resources via a
    /// </remarks>
    /// <param name="from"></param>
    /// <param name="vnetIntegration"></param>
    /// <param name="allResources"></param>
    public void BuildRelationships(AzureResource from, IEnumerable<AzureResource> allResources)
    {
        foreach (var storageAccount in _connectedStorageAccounts)
        {
            var storage = allResources.OfType<StorageAccount>()
                .SingleOrDefault(x => x.Name.ToLowerInvariant() == storageAccount.storageName);
            if (storage != null)
            {
                from.CreateLayer7Flow(allResources, storage, "uses",
                    hns => hns.Any(hn =>
                        hn.StartsWith(storageAccount.storageName) && hn.EndsWith(storageAccount.storageSuffix)),
                    Plane.Runtime);
            }
        }

        foreach (var databaseConnection in _databaseConnections)
        {
            //TODO check server name as-well
            var server = allResources.OfType<ManagedSqlServer>().SingleOrDefault(x =>
                x.CanIAccessYouOnThisHostName(databaseConnection.serverName));

            // string.Compare(x.Name, databaseConnection.serverName, StringComparison.InvariantCultureIgnoreCase) == 0);

            if (server != null)
            {
                from.CreateLayer7Flow(allResources, server, "sql",
                    hns => hns.Any(hn => hn.StartsWith(server.Name)), Plane.Runtime);
            }
        }

        foreach (var keyVaultReference in _keyVaultReferences)
        {
            //TODO KeyVault via private endpoint. Needs a generic way to look for a host that is accessed via private endpoints.

            //TODO check server name as-well
            var keyVault = allResources.OfType<KeyVault>().SingleOrDefault(x =>
                string.Compare(x.Name, keyVaultReference, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (keyVault != null)
            {
                from.CreateLayer7Flow(allResources, keyVault, "secrets",
                    hns => hns.Any(hn => keyVault.CanIAccessYouOnThisHostName(hn)), Plane.Runtime);
            }
        }

        allResources.OfType<ICanBeAccessedViaAHostName>()
            .Where(x => _hostNamesAccessedInAppSettings.Any(x.CanIAccessYouOnThisHostName))
            .ForEach(x =>
            {
                from.CreateLayer7Flow(allResources, (AzureResource)x, "calls",
                    hns => hns.Any(x.CanIAccessYouOnThisHostName), Plane.Runtime);
            });
    }
}