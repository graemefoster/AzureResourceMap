using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

internal class ManagedSqlServer : AzureResource, ICanBeAccessedViaAHostName
{
    public override bool IsPureContainer => false;
    public override string Image => "img/lib/azure2/databases/SQL_Server.svg";

    public string Hostname { get; set; }

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        Hostname = full["properties"]!.Value<string>("fullyQualifiedDomainName")!;
        return base.Enrich(full, additionalResources);
    }

    public void DiscoveredDatabase(ManagedSqlDatabase managedSqlDatabase)
    {
        OwnsResource(managedSqlDatabase);
    }

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        //contains to enable more specific connections like 'tcp:<server-name>,1433'
        return hostname.Contains(Hostname, StringComparison.InvariantCultureIgnoreCase);
    }
}