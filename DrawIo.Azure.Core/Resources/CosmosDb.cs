using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class CosmosDb : AzureResource, ICanBeAccessedViaHttp
{
    public override string Image => "img/lib/azure2/databases/Azure_Cosmos_DB.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        DocumentEndpointHost = full["properties"]!.Value<string>("documentEndpoint")?.GetHostNameFromUrlString() ?? null; 
        return base.Enrich(full, additionalResources);
    }

    public string? DocumentEndpointHost { get; set; }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        base.BuildRelationships(allResources);
    }

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return DocumentEndpointHost?.CompareTo(hostname.ToLowerInvariant()) == 0;
    }
}