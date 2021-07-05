using System;
using System.IO;
using System.Text;
using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;
using Node = Microsoft.Msagl.Drawing.Node;

namespace DrawIoSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var drawingGraph = new Graph();
            drawingGraph.AddNode("TEST 1");
            drawingGraph.AddNode("TEST 2");
            drawingGraph.AddNode("TEST 3");
            drawingGraph.AddNode("TEST 4");
            drawingGraph.AddNode("TEST 5");
            drawingGraph.AddEdge("TEST 1", "TEST 2");
            drawingGraph.AddEdge("TEST 2", "TEST 3");
            drawingGraph.AddEdge("TEST 1", "TEST 4");
            drawingGraph.AddEdge("TEST 3", "TEST 5");

            var subGraph = new Subgraph("Section02");
            subGraph.AddNode(drawingGraph.FindNode("TEST 3"));
            subGraph.AddNode(drawingGraph.FindNode("TEST 4"));
            
            var subGraph1 = new Subgraph("Section01");
            subGraph1.AddNode(drawingGraph.FindNode("TEST 5"));

            drawingGraph.RootSubgraph.AddSubgraph(subGraph1);
            subGraph1.AddSubgraph(subGraph);
            
            drawingGraph.CreateGeometryGraph();

            foreach (var node in drawingGraph.Nodes)
            {
                node.GeometryNode.BoundaryCurve =
                    CurveFactory.CreateRectangle(150, 50, new Point(0, 0));
            }

            var routingSettings = new Microsoft.Msagl.Core.Routing.EdgeRoutingSettings
            {
                UseObstacleRectangles = true,
                BendPenalty = 100,
                EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.StraightLine
            };

            var settings = new SugiyamaLayoutSettings
            {
                ClusterMargin = 50,
                PackingAspectRatio = 3,
                PackingMethod = Microsoft.Msagl.Core.Layout.PackingMethod.Columns,
                RepetitionCoefficientForOrdering = 0,
                EdgeRoutingSettings = routingSettings,
                NodeSeparation = 50,
                LayerSeparation = 150
            };

            LayoutHelpers.CalculateLayout(drawingGraph.GeometryGraph, settings, null);
            //TransformGraphByFlippingY(drawingGraph.GeometryGraph);

            var sb = new StringBuilder();
            var cellStyle = "rounded=0;whiteSpace=wrap;html=1;";

            foreach (var sg in drawingGraph.RootSubgraph.AllSubgraphsDepthFirst()) {

                if (sg.Id == "the root subgraph's boundary") continue;

                sb.AppendLine($@"
<mxCell id=""{Guid.NewGuid()}"" value="""" style=""rounded=0;whiteSpace=wrap;html=1;"" vertex=""1"" parent=""1"">
    <mxGeometry x=""{sg.BoundingBox.Left}"" y=""{sg.BoundingBox.Top - (sg.BoundingBox.Height / 2)}"" width=""{sg.BoundingBox.Width}"" height=""{sg.BoundingBox.Height}"" 
    as=""geometry"" />
</mxCell>
");
            }

            
            foreach (var node in drawingGraph.GeometryGraph.Nodes)
            {
                var nodeUserData = (Node) node.UserData;
                sb.AppendLine($@"
<mxCell id=""{nodeUserData.LabelText}"" value=""{nodeUserData.LabelText}"" style=""{cellStyle}"" vertex=""1"" parent=""1"">
    <mxGeometry x=""{node.Center.X}"" y=""{node.Center.Y}"" width=""{node.Width}"" height=""{node.Height}"" as=""geometry"" />
</mxCell>");
            }

            foreach (var edge in drawingGraph.GeometryGraph.Edges)
            {
                var target = (Node) edge.Target.UserData;
                var source = (Node) edge.Source.UserData;
                sb.AppendLine($@"
<mxCell id=""{Guid.NewGuid().ToString().Replace("-", "")}"" 
    style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=0.5;entryDx=0;entryDy=0;entryPerimeter=0;"" edge=""1"" parent=""1"" 
    source=""{source.Id}"" target=""{target.Id}"">
    <mxGeometry relative=""1"" as=""geometry"" />
</mxCell>");
            }
            
            var msGraph = @$"<mxGraphModel>
	<root>
		<mxCell id=""0"" />
		<mxCell id=""1"" parent=""0"" />
{sb}
	</root>
</mxGraphModel>";
            
            Console.WriteLine(msGraph);
            var directoryName = @"C:\Users\graemefoster\Documents\LINQPad Queries\AzureResourceManager\";
            File.WriteAllText(Path.Combine(directoryName, "graph.drawio"), msGraph);
        }
    }
}