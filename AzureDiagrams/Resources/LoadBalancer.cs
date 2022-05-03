using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class LoadBalancer : AzureResource, ICanInjectIntoASubnet, ICanExposePublicIPAddresses
{
    private IpConfigurations _frontendIpConfigurations = default!;
    private string[] _backendNics = default!;
    public override string Image => "img/lib/azure2/networking/Load_Balancers.svg";

    public string[] PublicIpAddresses => _frontendIpConfigurations.PublicIpAddresses;

    public string[] SubnetIdsIAmInjectedInto => _frontendIpConfigurations.SubnetAttachments;

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        _frontendIpConfigurations = new IpConfigurations(full, "frontendIPConfigurations");
        _backendNics = full["properties"]!["backendAddressPools"]!.SelectMany(x =>
                x["properties"]!["loadBalancerBackendAddresses"]?.Select(
                    lbba => lbba["properties"]!["networkInterfaceIPConfiguration"]?.Value<string>("id")!) ??
                Array.Empty<string>())
            .Select(x => string.Join('/', x.Split("/")[0..^2]))
            .ToArray();

        return base.Enrich(full, additionalResources);
    }


    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        _backendNics.ForEach(x =>
            CreateFlowTo(
                allResources.OfType<Nic>().Single(nic => nic.Id.Equals(x, StringComparison.InvariantCultureIgnoreCase)),
                "lb", Plane.All));
        base.BuildRelationships(allResources);
    }
}