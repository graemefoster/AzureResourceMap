using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class VM : AzureResource, IAssociateWithNic
{
    public override bool FetchFull => true;
    public Identity? Identity { get; set; }
    public override string Image => "img/lib/azure2/compute/Virtual_Machine.svg";
    public string SystemDiskId { get; private set; } = default!;
    public override string? ApiVersion => "2021-07-01";
    public string[] Nics { get; private set; } = default!;

    public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
    {
        SystemDiskId = jObject["properties"]!["storageProfile"]!["osDisk"]!["managedDisk"]!.Value<string>("id")!;
        Nics = jObject["properties"]!["networkProfile"]!["networkInterfaces"]!.Select(x => x.Value<string>("id")!)
            .ToArray();

        return Task.CompletedTask;
    }
    //
    // public override void Link(IEnumerable<AzureResource> allResources, GeometryGraph graph)
    // {
    //     var disk = allResources.OfType<Disk>().Single(x => String.Equals(x.Id, SystemDiskId, StringComparison.InvariantCultureIgnoreCase));
    //     Link(disk, graph);
    // }
}