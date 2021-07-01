using System.Collections.Generic;
using System.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class PrivateEndpoint : AzureResource
    {
        public override bool IsSpecific => true;
        public override string Image => "img/lib/azure2/networking/Private_Link.svg";
        
        
        public override IEnumerable<string> Link(IEnumerable<AzureResource> allResources)
        {
            var endpointLinks = allResources.OfType<App>().Where(x => x.AccessedViaPrivateEndpoint(this)).Select(x => base.Link(x));
            return endpointLinks;
        }
    }
}