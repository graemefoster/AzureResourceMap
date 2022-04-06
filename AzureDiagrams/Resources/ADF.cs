using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureDiagrams.Resources.Retrievers.Custom;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class ADF : AzureResource
{
    private JObject _linkedServices = default!;

    public override string Image => "img/lib/azure2/databases/Data_Factory.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        _linkedServices = additionalResources[AzureDataFactoryRetriever.LinkedServices]!;
        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var possibleConnections = new RelationshipHelper(
            _linkedServices["value"]!
                .SelectMany(x =>
                    x["properties"]!["typeProperties"]?.ToObject<Dictionary<string, object>>()
                        ?.Select(kvp => kvp.Value) ?? Array.Empty<object>())
                .ToArray());

        possibleConnections.Discover();
        possibleConnections.BuildRelationships(this, allResources);

        base.BuildRelationships(allResources);
    }
}