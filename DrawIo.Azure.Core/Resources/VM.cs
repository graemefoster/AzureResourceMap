using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Msagl.Core.Layout;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class VM : AzureResource
    {
        public override bool IsSpecific => true;
        public override bool FetchFull => true;
        public Identity? Identity { get; set; }
        public override string Image => "img/lib/azure2/compute/Virtual_Machine.svg";
        public string SystemDiskId { get; private set; }
        public string[] Nics { get; private set; }
        public override string? ApiVersion => "2021-07-01";

        public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
        {
            SystemDiskId = jObject["properties"]!["storageProfile"]!["osDisk"]!["managedDisk"]!.Value<string>("id")!;
            Nics = jObject["properties"]!["networkProfile"]!["networkInterfaces"]!.Select(x => x.Value<string>("id")!).ToArray();
            
            return Task.CompletedTask;
        }

        public override void Link(IEnumerable<AzureResource> allResources, GeometryGraph graph)
        {
            var disk = allResources.OfType<Disk>().Single(x => String.Equals(x.Id, SystemDiskId, StringComparison.InvariantCultureIgnoreCase));
            Link(disk, graph);
        }
    }
}