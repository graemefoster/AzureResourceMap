using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class VNet : AzureResource
{
    public Subnet[] Subnets { get; private set; } = default!;
    public List<PrivateDnsZone> PrivateDnsZones { get; } = new();

    public override string Image => "img/lib/azure2/networking/Virtual_Networks.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        Subnets = full["properties"]!["subnets"]!.Select(x => new Subnet
        {
            Name = x.Value<string>("name")!,
            AddressPrefix = x["properties"]!.Value<string>("addressPrefix")!,
            UdrId = x["properties"]!["routeTable"]?.Value<string>("id")
        }).ToArray();

        return Task.CompletedTask;
    }

    public void AssignPrivateDnsZone(PrivateDnsZone resource)
    {
        PrivateDnsZones.Add(resource);
    }

    private void InjectResourceInto(AzureResource resource, string subnet)
    {
        Subnets.Single(x => string.Compare(x.Name, subnet, StringComparison.InvariantCultureIgnoreCase) == 0)
            .ContainedResources.Add(resource);
        resource.ContainedByAnotherResource = true;
    }

    public void AssignNsg(NSG nsg, string subnet)
    {
        Subnets.Single(x => x.Name == subnet).NSGs.Add(nsg);
        nsg.ContainedByAnotherResource = true;
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources
            .OfType<ICanInjectIntoASubnet>()
            .Select(x => (resource: (AzureResource)x, subnets: SubnetsInsideThisVNet(x.SubnetIdsIAmInjectedInto)))
            .ForEach(r =>
                r.subnets.ForEach(s => InjectResourceInto(r.resource, s)));

        Subnets.Where(x => x.UdrId != null).ForEach(x =>
            InjectResourceInto(allResources.OfType<UDR>().Single(udr => udr.Id.Equals(x.UdrId)), x.Name));
        base.BuildRelationships(allResources);
    }

    private IEnumerable<string> SubnetsInsideThisVNet(string[] subnetIdsIAmInjectedInto)
    {
        return subnetIdsIAmInjectedInto.Where(x =>
                string.Compare(Id, string.Join('/', x.Split('/')[..^2]), StringComparison.InvariantCultureIgnoreCase) ==
                0)
            .Select(x => x.Split('/')[^1]);
    }

    /// <summary>
    ///     VMs can be associated to multiple nics, in different subnets. So you can choose to put it in either.
    /// </summary>
    /// <param name="vm"></param>
    /// <param name="optionalSubnetId"></param>
    public void GiveHomeToVirtualMachine(VM vm, string? optionalSubnetId = null)
    {
        if (string.IsNullOrEmpty(optionalSubnetId))
            OwnsResource(vm);
        else
            InjectResourceInto(vm, optionalSubnetId);
    }

    public class Subnet
    {
        public string Name { get; init; } = default!;
        public string? UdrId { get; init; }
        public string AddressPrefix { get; init; } = default!;

        public List<AzureResource> ContainedResources { get; } = new();

        public List<NSG> NSGs { get; } = new();

        public UDR? UDR { get; set; }
    }
}