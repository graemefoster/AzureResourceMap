﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Diagrams;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class ContainerApp : AzureResource, ICanBeAccessedViaHttp
{
    public string KubeEnvironmentId { get; set; } = default!;
    public override string Image => "img/lib/azure2/compute/Container_Instances.svg";

    public string IngressFqdn { get; private set; } = default!;

    public string[] DaprHostNames { get; private set; } = default!;

    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new AzureResourceNodeBuilder(this);
    }

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        KubeEnvironmentId = full["properties"]!.Value<string>("kubeEnvironmentId")!;
        IngressFqdn = full["properties"]!["configuration"]!["ingress"]!.Value<string>("fqdn")!;
        DaprHostNames = full["properties"]!["template"]?["dapr"]?["components"]?
            .SelectMany(component =>
                component["metadata"]?
                    .Values<string>("value")
                    .Where(x => x != null)
                    .Select(x => x!.GetHostNameFromUrlStringOrNull())
                    .Where(x => x != null)
                    .Select(x => x!)
                    .ToArray() ?? Array.Empty<string>()).ToArray() ?? Array.Empty<string>();

        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var kubeEnvironment = allResources.OfType<ContainerAppEnvironment>().SingleOrDefault(x =>
            string.Compare(x.Id, KubeEnvironmentId, StringComparison.InvariantCultureIgnoreCase) == 0);

        allResources.OfType<ICanBeAccessedViaHttp>().Where(x => DaprHostNames.Any(x.CanIAccessYouOnThisHostName)).ForEach(x => CreateFlowTo((AzureResource)x, "uses"));
        
        kubeEnvironment?.DiscoveredContainerApp(this);
    }

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return string.Compare(IngressFqdn, hostname, StringComparison.InvariantCultureIgnoreCase) == 0;
    }
}