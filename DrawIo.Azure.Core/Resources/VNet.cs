using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
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
            foreach (var subnet in Subnets)
            {
                var emptyContents = new Node(CurveFactory.CreateRectangle(150, 75, new Point()));
                var cluster = new Cluster(subnet.ContainedResources.Select(x => x.Node).Union(new[] { emptyContents }))
                {
                    BoundaryCurve = CurveFactory.CreateRectangle(150, 75, new Point()),
                    UserData = subnet.Name
                };
                graph.Nodes.Add(emptyContents);
                graph.Nodes.Add(cluster);
                _clusters.Add(cluster);
            }

            var vnetCluster = new Cluster(new[] { Node }, _clusters)
            {
                BoundaryCurve = CurveFactory.CreateRectangle(150, 75, new Point()), UserData = Name
            };
            graph.Nodes.Add(vnetCluster);
            _clusters.Add(vnetCluster);
        }

        public override IEnumerable<string> ToDrawIo()
        {
            return base.ToDrawIo().Union(
                _clusters.Select(x =>
                {
                    var boundingBoxWidth = x.BoundingBox.Width;
                    var boundingBoxHeight = x.BoundingBox.Height;
                    var boundingBoxLeft = x.BoundingBox.Left - (x.ClusterParent?.BoundingBox.Left ?? 0);
                    var boundingBoxTop = x.BoundingBox.Bottom - (x.ClusterParent?.BoundingBox.Bottom ?? 0);
                    var nodeUserData = (string)x.UserData;

                    return $@"
<mxCell id=""{(string)x.UserData}"" value=""{nodeUserData}"" style=""rounded=0;whiteSpace=wrap;html=1;fillColor=#dae8fc"" vertex=""1"" parent=""{(x.ClusterParent == null ? "1" : x.ClusterParent.UserData)}"">
    <mxGeometry x=""{boundingBoxLeft}"" y=""{boundingBoxTop}"" width=""{boundingBoxWidth}"" height=""{boundingBoxHeight}"" 
    as=""geometry"" />
</mxCell>";
                }));
        }
    }
}