using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Resources
{
    class PIP : AzureResource
    {
        public override bool IsSpecific => true;
        public override string Image => "img/lib/azure2/networking/Public_IP_Addresses.svg";

        public override IEnumerable<string> Link(IEnumerable<AzureResource> allResources, GeometryGraph graph)
        {
            return allResources.OfType<Nic>().Where(x => x.ExposedBy(this)).Select(x => Link(x, graph));
        }
    }
}