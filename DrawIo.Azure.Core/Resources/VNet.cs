using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Diagrams;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class VNet : AzureResource
{
    public Subnet[] Subnets { get; private set; } = default!;
    public List<PrivateDnsZone> PrivateDnsZones { get; } = new();

    public override string Image => "img/lib/azure2/networking/Virtual_Networks.svg";

    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new VNetDiagramResourceBuilder(this);
    }

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        Subnets = full["properties"]!["subnets"]!.Select(x => new Subnet
        {
            Name = x.Value<string>("name")!,
            AddressPrefix = x["properties"]!.Value<string>("addressPrefix")!
        }).ToArray();

        return Task.CompletedTask;
    }

    public void AssignPrivateDnsZone(PrivateDnsZone resource)
    {
        PrivateDnsZones.Add(resource);
    }

    public void InjectResourceInto(AzureResource resource, string subnet)
    {
        Subnets.Single(x => x.Name == subnet).ContainedResources.Add(resource);
        resource.ContainedByAnotherResource = true;
    }

    public void AssignNsg(NSG nsg, string subnet)
    {
        Subnets.Single(x => x.Name == subnet).NSGs.Add(nsg);
        nsg.ContainedByAnotherResource = true;
    }

    public class Subnet
    {
        public string Name { get; init; } = default!;
        public string AddressPrefix { get; init; } = default!;
        internal List<AzureResource> ContainedResources { get; } = new();

        public List<NSG> NSGs { get; } = new();
    }
}