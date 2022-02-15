using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class VMSS : AzureResource, ICanInjectIntoASubnet
{
    public override string Image => "img/lib/azure2/compute/VM_Scale_Sets.svg";

    public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
    {
        var nicConfigurations =
            jObject["properties"]!["virtualMachineProfile"]!["networkProfile"]!["networkInterfaceConfigurations"]?.SelectMany(x => x["properties"]!["ipConfigurations"]!);

        LoadBalancerRelationships = nicConfigurations.SelectMany(GetLoadBalancersFromIpConfiguration).Distinct();
        SubnetIdsIAmInjectedInto = nicConfigurations.Select(GetSubnetFromIpConfiguration).Where(x => x != null).Distinct().Select(x => x!).ToArray();

        return Task.CompletedTask;
    }

    private string? GetSubnetFromIpConfiguration(JToken ipConfiguration)
    {
        return ipConfiguration["properties"]!["subnet"]?.Value<string>("id");
    }

    public IEnumerable<string> LoadBalancerRelationships { get; private set; } = default!;

    private IEnumerable<string> GetLoadBalancersFromIpConfiguration(JToken ipConfiguration)
    {
        var lbBackEndPools = ipConfiguration["properties"]!["loadBalancerBackendAddressPools"]
                                 ?.Select(lbbap => string.Join('/',
                                     lbbap.Value<string>("id")?.Split('/')[..^2] ?? Array.Empty<string>())) ??
                             Array.Empty<string>();

        var lbNatPools = ipConfiguration["properties"]!["loadBalancerInboundNatPools"]
                             ?.Select(lbbap => string.Join('/',
                                 lbbap.Value<string>("id")?.Split('/')[..^2] ?? Array.Empty<string>())) ??
                         Array.Empty<string>();

        return lbBackEndPools.Union(lbNatPools);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<LoadBalancer>()
            .Where(x => LoadBalancerRelationships.Contains(x.Id, StringComparer.InvariantCultureIgnoreCase))
            .ForEach(lb => lb.CreateFlowTo(this, "Connects"));
        base.BuildRelationships(allResources);
    }

    public string[] SubnetIdsIAmInjectedInto { get; private set; }
}