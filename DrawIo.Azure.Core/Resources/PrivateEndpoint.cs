using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Core.Layout;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class PrivateEndpoint : AzureResource
    {
        public override bool IsSpecific => true;
        public override bool FetchFull => true;
        public override string Image => "img/lib/azure2/networking/Private_Link.svg";

        public string[] Nics { get; private set; }

        public override IEnumerable<string> Link(IEnumerable<AzureResource> allResources, GeometryGraph graph)
        {
            var endpointLinks =
                allResources
                    .OfType<App>().Where(x => x.AccessedViaPrivateEndpoint(this))
                    .Select(x => base.Link(x, graph))
                    .Union(
                        allResources
                            .OfType<KeyVault>().Where(x => x.AccessedViaPrivateEndpoint(this))
                            .Select(x => base.Link(x, graph))
                    );
            return endpointLinks;
        }

        public override void Enrich(JObject full)
        {
            Nics = full["properties"]["networkInterfaces"].Select(x => x.Value<string>("id")).ToArray();
        }
    }
}