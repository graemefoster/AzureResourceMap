﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Resources.Retrievers;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class APIm : AzureResource, IUseManagedIdentities, ICanBeAccessedViaHttp
{
    public Identity? Identity { get; set; }
    public override string Image => "img/lib/azure2/app_services/API_Management_Services.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        HostNames = full["properties"]!["hostnameConfigurations"]!.Select(x => x.Value<string>("hostName")!).ToArray();
        Backends = additionalResources[ApimServiceResourceRetriever.BackendList]["value"]?.Select(x => x["properties"]!.Value<string>("url")!)
            .Select(x => new Uri(x).Host)
            .ToArray() ?? Array.Empty<string>();
        
        return base.Enrich(full, additionalResources);
    }

    public string[] Backends { get; set; }

    public string[] HostNames { get; set; } = default!;

    public bool DoYouUseThisUserAssignedClientId(string id)
    {
        return Identity?.UserAssignedIdentities?.Keys.Any(k => string.Compare(k, id, StringComparison.InvariantCultureIgnoreCase) == 0) ?? false;
    }

    public void CreateFlowToMe(UserAssignedManagedIdentity userAssignedManagedIdentity)
    {
        CreateFlowTo(userAssignedManagedIdentity);
    }

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return HostNames.Any(hn => string.Compare(hostname, hn, StringComparison.InvariantCultureIgnoreCase) == 0);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var distinctAccessedHosts = allResources.OfType<ICanBeAccessedViaHttp>().Where(x => Backends.Any(x.CanIAccessYouOnThisHostName)).Distinct();
        distinctAccessedHosts.ForEach(x => CreateFlowTo((AzureResource)x));
    }
}