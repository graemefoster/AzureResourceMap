using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

internal class ContainerApp : AzureResource, ICanBeAccessedViaAHostName
{
    public string ContainerAppEnvironmentId { get; set; } = default!;
    public override string Image => "img/lib/azure2/compute/Container_Instances.svg";

    public string IngressFqdn { get; private set; } = default!;


    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return string.Compare(IngressFqdn, hostname, StringComparison.InvariantCultureIgnoreCase) == 0;
    }

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        ContainerAppEnvironmentId = full["properties"]!.Value<string>("managedEnvironmentId")!;
        IngressFqdn = full["properties"]!["configuration"]!["ingress"]!.Value<string>("fqdn")!;
        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var kubeEnvironment = allResources.OfType<ContainerAppEnvironment>().SingleOrDefault(x =>
            string.Compare(x.Id, ContainerAppEnvironmentId, StringComparison.InvariantCultureIgnoreCase) == 0);

        kubeEnvironment?.DiscoveredContainerApp(this);
        base.BuildRelationships(allResources);
    }
}