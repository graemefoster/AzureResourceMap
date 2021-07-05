using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Resources
{
    class ASP : AzureResource, IContainResources
    {
        public override bool IsSpecific => true;
        public string Kind { get; set; }
        public override string Image => "img/lib/azure2/app_services/App_Service_Plans.svg";

        public void Group(GeometryGraph graph, IEnumerable<AzureResource> allResources)
        {
            var subNodes = new[] {Node}.Union(allResources.OfType<App>().Where(x =>
                    String.Equals(Id, x.Properties.ServerFarmId, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Node));
            
            var subgraph = new Cluster(subNodes.ToArray());
            // subgraph.AddClusterParent(graph.RootCluster);
            // subgraph.BoundaryCurve = CurveFactory.CreateRectangle(500, 150, new Point(250, 75));
            // foreach (var node in subgraph.Nodes)
            // {
            //     graph.Nodes.Remove(node);
            // }
            //
            graph.Nodes.Add(subgraph);
        }
    }
}