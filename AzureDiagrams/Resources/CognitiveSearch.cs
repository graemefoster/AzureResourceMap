using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class CognitiveSearch : AzureResource, ICanBeAccessedViaAHostName
{
    private IEnumerable<string> _resourcesAccessOverPrivateLink;
    public override string Image => "img/lib/azure2/app_services/Search_Services.svg";

    public string HostName { get; set; } = default!;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return string.Compare(HostName, hostname, StringComparison.InvariantCultureIgnoreCase) == 0
               || Name.Equals(hostname, StringComparison.InvariantCultureIgnoreCase);
        ;
    }

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        HostName = $"{Name.ToLowerInvariant()}.search.windows.net";

        _resourcesAccessOverPrivateLink = full["properties"]!["sharedPrivateLinkResources"]?
            .Select(x => x["properties"]!.Value<string>("privateLinkResourceId")!) ?? []; 
        
        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        _resourcesAccessOverPrivateLink.ForEach(x =>
        {
            var resource = allResources.SingleOrDefault(r => r.Id.ToLowerInvariant() == x.ToLowerInvariant());
            if (resource != null)
            {
                CreateFlowTo(resource, "Private Link", Plane.Runtime);
            }
        });
        base.BuildRelationships(allResources);
    }
}