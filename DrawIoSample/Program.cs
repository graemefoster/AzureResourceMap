using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;

namespace DrawIoSample;

internal class Program
{
    private static void Main(string[] args)
    {
        var drawingGraph = new GeometryGraph();
        var test1 = new Node(CurveFactory.CreateRectangle(150, 75, new Point())) { UserData = "test1" };
        var test2 = new Node(CurveFactory.CreateRectangle(150, 75, new Point())) { UserData = "test2" };
        var test3 = new Node(CurveFactory.CreateRectangle(150, 75, new Point())) { UserData = "test3" };
        var test4 = new Node(CurveFactory.CreateRectangle(150, 75, new Point())) { UserData = "test4" };
        var test5 = new Node(CurveFactory.CreateRectangle(150, 75, new Point())) { UserData = "test5" };
        drawingGraph.Nodes.Add(test1);
        drawingGraph.Nodes.Add(test2);
        drawingGraph.Nodes.Add(test3);
        drawingGraph.Nodes.Add(test4);
        drawingGraph.Nodes.Add(test5);
        drawingGraph.Edges.Add(new Edge(test1, test2));
        drawingGraph.Edges.Add(new Edge(test1, test4));
        drawingGraph.Edges.Add(new Edge(test1, test5));
        drawingGraph.Edges.Add(new Edge(test2, test3));
        drawingGraph.Edges.Add(new Edge(test3, test5));

        var cluster2 = new Cluster(new[] { test5 })
            { UserData = "cluster2", BoundaryCurve = CurveFactory.CreateRectangle(200, 100, new Point()) };

        var cluster1 = new Cluster(new[] { test3, test4 }, new[] { cluster2 })
            { UserData = "cluster1", BoundaryCurve = CurveFactory.CreateRectangle(200, 100, new Point()) };

        drawingGraph.RootCluster.AddChild(cluster1);

        var routingSettings = new EdgeRoutingSettings
        {
            UseObstacleRectangles = true,
            BendPenalty = 10,
            EdgeRoutingMode = EdgeRoutingMode.StraightLine
        };

        var settings = new SugiyamaLayoutSettings
        {
            ClusterMargin = 50,
            PackingAspectRatio = 3,
            PackingMethod = PackingMethod.Compact,
            RepetitionCoefficientForOrdering = 0,
            LayerSeparation = 50,
            EdgeRoutingSettings = routingSettings,
            NodeSeparation = 50
        };

        LayoutHelpers.CalculateLayout(drawingGraph, settings, null);

        var sb = new StringBuilder();
        var cellStyle = "rounded=0;whiteSpace=wrap;html=1;";

        var flattenedNodesAndClusters = drawingGraph.GetFlattenedNodesAndClusters().ToArray();

        int LayoutOrder(Node node)
        {
            var order = 0;
            var parent = node.ClusterParent;
            while (parent != null)
            {
                order++;
                parent = parent.ClusterParent;
            }

            return order;
        }

        foreach (var node in flattenedNodesAndClusters.OrderBy(LayoutOrder))
        {
            var nodeUserData = (string)node.UserData;
            var boundingBoxWidth = node.BoundingBox.Width;
            var boundingBoxHeight = node.BoundingBox.Height;
            var boundingBoxLeft = node.BoundingBox.Left;
            var boundingBoxTop = node.BoundingBox.Bottom;

            if (node.ClusterParent != null)
            {
                boundingBoxLeft -= node.ClusterParent.BoundingBox.Left;
                boundingBoxTop -= node.ClusterParent.BoundingBox.Bottom;
            }

            Console.WriteLine($"Parent::{node.ClusterParent?.UserData}: {nodeUserData} - {node.BoundingBox}");
            sb.AppendLine($@"
<mxCell id=""{nodeUserData}"" value=""{nodeUserData}"" style=""rounded=0;whiteSpace=wrap;html=1;{(node is Cluster ? "verticalAlign=top" : "")}"" vertex=""1"" parent=""{(node.ClusterParent == null || node.ClusterParent == drawingGraph.RootCluster ? "1" : (string)node.ClusterParent.UserData)}"">
    <mxGeometry x=""{boundingBoxLeft}"" y=""{boundingBoxTop}"" width=""{boundingBoxWidth}"" height=""{boundingBoxHeight}"" 
    as=""geometry"" />
</mxCell>
");
        }

        foreach (var edge in drawingGraph.Edges)
        {
            var target = (string)edge.Target.UserData;
            var source = (string)edge.Source.UserData;
            sb.AppendLine($@"
<mxCell id=""{Guid.NewGuid().ToString().Replace("-", "")}"" 
    style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=0.5;entryDx=0;entryDy=0;entryPerimeter=0;"" edge=""1"" parent=""1"" 
    source=""{source}"" target=""{target}"">
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
        var directoryName = @"C:\temp\";
        File.WriteAllText(Path.Combine(directoryName, "graph2.drawio"), msGraph);
    }
}