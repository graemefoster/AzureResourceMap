using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

internal class ManagedSqlDatabase : AzureResource
{
    public override string Image => "img/lib/azure2/databases/SQL_Database.svg";

    public string ServerId { get; private set; } = default!;

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        ServerId = string.Join('/', Id.Split('/')[..^2]);
        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var server = allResources.OfType<ManagedSqlServer>().SingleOrDefault(x =>
            string.Compare(ServerId, x.Id, StringComparison.InvariantCultureIgnoreCase) == 0);
        if (server != null) server.DiscoveredDatabase(this);
        base.BuildRelationships(allResources);
    }
}