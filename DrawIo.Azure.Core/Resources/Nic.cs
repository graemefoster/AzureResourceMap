using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Core.Layout;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class Nic : AzureResource
    {
        public override bool IsSpecific => true;
        public override bool FetchFull => true;
        public override string Image => "img/lib/azure2/networking/Network_Interfaces.svg";
        public string[] PublicIpAddresses {get; set; }

        public override IEnumerable<string> Link(IEnumerable<AzureResource> allResources, GeometryGraph graph)
        {
            return 
                allResources.OfType<PrivateEndpoint>().Where(x => x.Nics.Contains(Id)).Select(y => Link(y, graph))
                    .Union(allResources.OfType<VM>().Where(x => x.Nics.Contains(Id)).Select(y => Link(y, graph)));
        }

        public override void Enrich(JObject full)
        {
            PublicIpAddresses = full["properties"]["ipConfigurations"]
                .Select(x => x["properties"]["publicIPAddress"] != null ? x["properties"]["publicIPAddress"].Value<string>("id").ToLowerInvariant() : null)
                .Where(x => x != null)
                .ToArray();
        }

        public bool ExposedBy(PIP pip)
        {
            return PublicIpAddresses.Contains(pip.Id.ToLowerInvariant());
        }
    }
}