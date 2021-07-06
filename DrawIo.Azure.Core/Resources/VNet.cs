using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Msagl.Core.Layout;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class VNet : AzureResource, IContainResources
    {
        private IList<Cluster> _clusters = new List<Cluster>();
        public override bool IsSpecific => true;
        public override bool FetchFull => true;
        public override string ApiVersion => "2021-02-01";
        public Subnet[] Subnets { get; set; }

        public override string Image => "img/lib/azure2/networking/Virtual_Networks.svg";

        public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
        {
            Subnets = full["properties"]!["subnets"]!.Select(x => new Subnet
            {
                Name = x.Value<string>("name")!,
                AddressPrefix = x["properties"]!.Value<string>("addressPrefix")!
            }).ToArray();
            
            return Task.CompletedTask;
        }

        public class Subnet
        {
            public string Name { get; set; }
            public string AddressPrefix { get; set; }
            internal List<AzureResource> ContainedResources { get; } = new();
        }

        public void AddToVNet(AzureResource resource, string subnet)
        {
            Subnets.Single(x => x.Name == subnet).ContainedResources.Add(resource);
        }

        public void Group(GeometryGraph graph, IEnumerable<AzureResource> allResources)
        {
            var vnetCluster = new Cluster( new[] {Node});
            foreach (var subnet in Subnets)
            {
                var cluster = new Cluster(subnet.ContainedResources.Select(x => x.Node));
                vnetCluster.AddChild(cluster);
                _clusters.Add(cluster);
            }

            graph.Nodes.Add(vnetCluster);
            
            _clusters.Add(vnetCluster);

        }


        public override IEnumerable<string> ToDrawIo()
        {
            return base.ToDrawIo().Union(
                _clusters.Select(x =>
                    $@"
<mxCell id=""{Guid.NewGuid()}"" value="""" style=""rounded=0;whiteSpace=wrap;html=1;"" vertex=""1"" parent=""1"">
    <mxGeometry x=""{x.BoundingBox.Left}"" y=""{x.BoundingBox.Top - (x.BoundingBox.Height / 2)}"" width=""{x.BoundingBox.Width}"" height=""{x.BoundingBox.Height}"" 
    as=""geometry"" />
</mxCell>"
                ));
        }
    }
}