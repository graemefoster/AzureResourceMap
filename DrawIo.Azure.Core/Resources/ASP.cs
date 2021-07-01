using System.Collections.Generic;
using System.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class ASP : AzureResource
    {
        public override bool IsSpecific => true;
        public string Kind { get; set; }
        public override string Image => "img/lib/azure2/app_services/App_Service_Plans.svg";

        public override IEnumerable<string> Link(IEnumerable<AzureResource> allResources)
        {
            var links = allResources.OfType<App>().Where(x => x.IsInAppService(this)).Select(x => base.Link(x)).ToArray();
            return links;
        }
    }
}