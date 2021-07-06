using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Msagl.Core.Layout;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class Nic : AzureResource
    {
        public override bool IsSpecific => true;
        public override bool FetchFull => true;
        public override string Image => "img/lib/azure2/networking/Network_Interfaces.svg";
        public string[] PublicIpAddresses { get; set; }

        public override void Link(IEnumerable<AzureResource> allResources, GeometryGraph graph)
        {
            allResources.OfType<PrivateEndpoint>().Where(x => x.Nics.Contains(Id)).ForEach(y => Link(y, graph));
            allResources.OfType<VM>().Where(x => x.Nics.Contains(Id)).ForEach(y => Link(y, graph));

            var subnets = _networkAttachments
                .Select(x =>
                {
                    var segments = x.Split('/');
                    return new {vnet = segments.ElementAt(segments.Length - 3), subnet = segments.Last()};
                })
                .ToArray();
            foreach (var subnet in subnets)
            {
                allResources.OfType<VNet>().Single(x => x.Name == subnet.vnet).AddToVNet(this, subnet.subnet);
            }
        }

        public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
        {
            PublicIpAddresses = jObject["properties"]!["ipConfigurations"]!
                .Select(x =>
                    x["properties"]!["publicIPAddress"] != null
                        ? x["properties"]!["publicIPAddress"]!.Value<string>("id")!.ToLowerInvariant()
                        : null)
                .Where(x => x != null)
                .ToArray()!;

            _networkAttachments = jObject["properties"]!["ipConfigurations"]!
                .Select(x =>
                    x["properties"]!["subnet"] != null
                        ? x["properties"]!["subnet"]!.Value<string>("id")!.ToLowerInvariant()
                        : null)
                .Where(x => x != null)
                .Select(x => x!);
            
            return Task.CompletedTask;
        }

        private IEnumerable<string> _networkAttachments { get; set; }

        public bool ExposedBy(PIP pip)
        {
            return PublicIpAddresses.Contains(pip.Id.ToLowerInvariant());
        }
    }
}