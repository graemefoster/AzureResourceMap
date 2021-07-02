using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void Enrich(JObject full)
        {
            SystemDiskId = full["properties"]["storageProfile"]["osDisk"]["managedDisk"].Value<string>("id");
            Nics = full["properties"]["networkProfile"]["networkInterfaces"].Select(x => x.Value<string>("id")).ToArray();
        }

        public override IEnumerable<string> Link(IEnumerable<AzureResource> allResources, GeometryGraph graph)
        {
            var disk = allResources.OfType<Disk>().Single(x => String.Equals(x.Id, SystemDiskId, StringComparison.InvariantCultureIgnoreCase));
            return new[] {Link(disk, graph)};
        }
    }
}