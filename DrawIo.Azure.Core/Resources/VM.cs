using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class VM : AzureResource, IAssociateWithNic, IUseManagedIdentities
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
    }

    public void AddExtension(VMExtension vmExtension)
    {
        OwnsResource(vmExtension);
    }

    public bool DoYouUseThisUserAssignedClientId(string id)
    {
        return Identity?.UserAssignedIdentities?.Keys.Any(k => string.Compare(k, id, StringComparison.InvariantCultureIgnoreCase) == 0) ?? false;
    }

    public void CreateFlowToMe(UserAssignedManagedIdentity userAssignedManagedIdentity)
    {
        CreateFlowTo(userAssignedManagedIdentity);
    }
}