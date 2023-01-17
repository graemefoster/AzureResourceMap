using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

[DebuggerDisplay("{Type}/{Name}")]
public class StorageAccount : AzureResource, ICanBeAccessedViaAHostName
{
    public StorageAccount(string id, string name, string[] hostNames)
    {
        Id = id;
        Name = name;
        HostNames = hostNames;
    }

    /// <summary>
    /// Used for json deserialization
    /// </summary>
    [JsonConstructor]
    public StorageAccount()
    {
    }

    public string[] HostNames { get; private set; } = default!;
    public override string Image => "img/lib/azure2/storage/Storage_Accounts.svg";

    public override Task Enrich(JObject jObject, Dictionary<string, JObject?> additionalResources)
    {
        HostNames = jObject["properties"]!["primaryEndpoints"]?.ToObject<Dictionary<string, string>>()?.Values.Select(x => x.GetHostNameFromUrlString()).ToArray() ??
                    Array.Empty<string>();
        return base.Enrich(jObject, additionalResources);
    }

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return HostNames.Contains(hostname, StringComparer.InvariantCultureIgnoreCase);
    }
}