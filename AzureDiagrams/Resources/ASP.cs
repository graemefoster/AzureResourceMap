using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class ASP : AzureResource
{
    public override string Image => "img/lib/azure2/app_services/App_Service_Plans.svg";

    public string? ASE { get; private set; }

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        var hostingEnvironmentProfile = full["properties"]!["hostingEnvironmentProfile"]!;
        if (hostingEnvironmentProfile.Type != JTokenType.Null)
        {
            ASE = hostingEnvironmentProfile.Value<string>("id");
        }
        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var apps = allResources.OfType<App>().Where(x =>
            string.Equals(Id, x.ServerFarmId, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        apps.ForEach(OwnsResource);
        base.BuildRelationships(allResources);
    }
}