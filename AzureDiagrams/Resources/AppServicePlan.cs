using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class AppServicePlan : AzureResource
{
    public override string Image => "img/lib/azure2/app_services/App_Service_Plans.svg";
    public override string? Fill => "#DAE8FC";
    public string? ASE { get; private set; }
    
    public AppServicePlan(string id, string name)
    {
        Id = id;
        Name = name;
        Type = "microsoft.web/serverfarms";
    }

    /// <summary>
    /// Used for json deserialization
    /// </summary>
    [JsonConstructor]
    public AppServicePlan()
    {
    }


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
        var apps = allResources.OfType<AppServiceApp>()
            .Where(x => string.Equals(Id, x.ServerFarmId, StringComparison.InvariantCultureIgnoreCase))
            .Where(x => IsNotSlotContainerByAnotherApp(x, allResources))
            .ToArray();
        apps.ForEach(OwnsResource);
        base.BuildRelationships(allResources);
    }

    private bool IsNotSlotContainerByAnotherApp(AppServiceApp appServiceApp, IEnumerable<AzureResource> allResources)
    {
        return !appServiceApp.IsSlotContainerByAnotherApp(allResources, out _);
    }
}