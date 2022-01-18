using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Resources.Retrievers.Custom;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class APIm : AzureResource, IUseManagedIdentities, ICanBeAccessedViaAHostName, ICanExposePublicIPAddresses,
    ICanInjectIntoASubnet
{
    public Identity? Identity { get; set; }
    public override string Image => "img/lib/azure2/app_services/API_Management_Services.svg";

    public string[] Backends { get; set; } = default!;

    public string[] HostNames { get; set; } = default!;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return HostNames.Any(hn => string.Compare(hostname, hn, StringComparison.InvariantCultureIgnoreCase) == 0);
    }

    public string[] PublicIpAddresses { get; private set; } = default!;
    public string[] SubnetIdsIAmInjectedInto { get; private set; } = default!;

    public bool DoYouUseThisUserAssignedClientId(string id)
    {
        return Identity?.UserAssignedIdentities?.Keys.Any(k =>
            string.Compare(k, id, StringComparison.InvariantCultureIgnoreCase) == 0) ?? false;
    }

    public void CreateManagedIdentityFlowBackToMe(UserAssignedManagedIdentity userAssignedManagedIdentity)
    {
        CreateFlowTo(userAssignedManagedIdentity, "AAD Identity", FlowEmphasis.LessImportant);
    }

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        HostNames = full["properties"]!["hostnameConfigurations"]!.Select(x => x.Value<string>("hostName")!).ToArray();

        Backends = additionalResources[ApimServiceResourceRetriever.BackendList]["value"]
            ?.Select(x => x["properties"]!.Value<string>("url")!)
            .Select(x => new Uri(x).Host)
            .ToArray() ?? Array.Empty<string>();

        PublicIpAddresses = full["properties"]!["publicIPAddresses"]?
                                .Select((x, idx) => $"{Name}.pip.{idx}").ToArray() ??
                            Array.Empty<string>();

        var subnet = full["properties"]!["virtualNetworkConfiguration"]?.Value<string>("subnetResourceId");
        SubnetIdsIAmInjectedInto = subnet == null ? new[] { subnet! } : Array.Empty<string>();

        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        Backends.ForEach(x => this.CreateFlowToHostName(allResources, x, "calls"));
    }

    public override IEnumerable<AzureResource> DiscoverNewNodes()
    {
        foreach (var publicIpAddress in PublicIpAddresses)
        {
            yield return new PIP { Id = publicIpAddress, Name = publicIpAddress };
        }
    }
}