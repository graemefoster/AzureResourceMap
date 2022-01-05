// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Microsoft.Msagl.Core.Layout;
//
// namespace DrawIo.Azure.Core.Resources
// {
//     class ASP : AzureResource, IContainResources
//     {
//         public override bool IsSpecific => true;
//         public string Kind { get; set; }
//         public override string Image => "img/lib/azure2/app_services/App_Service_Plans.svg";
//         private Cluster _cluster;
//
//         public void CreateNodes(GeometryGraph graph, IEnumerable<AzureResource> allResources)
//         {
//             var subNodes = new[] {Node}.Union(allResources.OfType<App>().Where(x =>
//                     String.Equals(Id, x.Properties.ServerFarmId, StringComparison.InvariantCultureIgnoreCase))
//                 .Select(x => x.Node));
//             
//             _cluster = new Cluster(subNodes.ToArray());
//             graph.Nodes.Add(_cluster);
//         }
//
//         public override IEnumerable<string> ToDrawIo()
//         {
//             return base.ToDrawIo().Union(new []
//             {
//                 $@"
// <mxCell id=""{Guid.NewGuid()}"" value="""" style=""rounded=0;whiteSpace=wrap;html=1;"" vertex=""1"" parent=""1"">
//     <mxGeometry x=""{_cluster.BoundingBox.Left}"" y=""{_cluster.BoundingBox.Top - (_cluster.BoundingBox.Height / 2)}"" width=""{_cluster.BoundingBox.Width}"" height=""{_cluster.BoundingBox.Height}"" 
//     as=""geometry"" /></mxCell>"
//             });
//         }
//     }
// }

