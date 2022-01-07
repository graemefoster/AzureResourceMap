using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class VM : AzureResource, IAssociateWithNic, IUseManagedIdentities
{
    public Identity? Identity { get; set; }
    public override string Image => "img/lib/azure2/compute/Virtual_Machine.svg";
    public string SystemDiskId { get; private set; } = default!;
    public string[] Nics { get; private set; } = default!;

    public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
    {
        SystemDiskId = jObject["properties"]!["storageProfile"]!["osDisk"]!["managedDisk"]!.Value<string>("id")!;
        Nics = jObject["properties"]!["networkProfile"]!["networkInterfaces"]!.Select(x => x.Value<string>("id")!)
            .ToArray();

        return Task.CompletedTask;
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var disk = allResources.OfType<Disk>()
            .Single(x => string.Equals(x.Id, SystemDiskId, StringComparison.InvariantCultureIgnoreCase));
        CreateFlowTo(disk);
        OwnsResource(disk);

        var allNics = Nics.Select(nic =>
            allResources.OfType<Nic>().Single(x => x.Id.Equals(nic, StringComparison.InvariantCultureIgnoreCase)));

        var injectedSubnets = allNics.SelectMany(nic => nic.SubnetIdsIAmInjectedInto).ToArray();
        if (injectedSubnets.Length == 1)
        {
            var vnetId = string.Join('/', injectedSubnets[0].Split('/')[..^2]);
            var vnet = allResources.OfType<VNet>().Single(x => x.Id.Equals(vnetId, StringComparison.InvariantCultureIgnoreCase));
            vnet.GiveHomeToVirtualMachine(this, injectedSubnets[0].Split('/')[^1]);
        }
        else
        {
            //inject the VM into the VNet... It can be in multiple subnets so it feels weird to try put it into each
            var vnets = injectedSubnets.Select(sn => allResources.OfType<VNet>().Single(x =>
                    x.Id.Equals(string.Join('/', sn.Split('/')[..^2]), StringComparison.InvariantCultureIgnoreCase)))
                .Distinct();
            vnets.ForEach(vnet => vnet.GiveHomeToVirtualMachine(this));
        }
    }

    public void AddExtension(VMExtension vmExtension)
    {
        OwnsResource(vmExtension);
    }

    public bool DoYouUseThisUserAssignedClientId(string id)
    {
        return Identity?.UserAssignedIdentities?.Keys.Any(k => string.Compare(k, id, StringComparison.InvariantCultureIgnoreCase) == 0) ?? false;
    }

    public void CreateFlowBackToMe(UserAssignedManagedIdentity userAssignedManagedIdentity)
    {
        CreateFlowTo(userAssignedManagedIdentity);
    }
}