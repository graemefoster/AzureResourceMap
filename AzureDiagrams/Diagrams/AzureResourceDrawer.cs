using System;
using DrawIo.Azure.Core.Resources;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Diagrams;

internal static class AzureResourceDrawer
{
    public static Node CreateSimpleRectangleNode(string type, string name, string id,
        string backgroundColour = "#dae8fc", TextAlignment textAlignment = TextAlignment.Middle)
    {
        var node = new Node(CurveFactory.CreateRectangle(150, 75, new Point())) { UserData = name };
        node.UserData = new CustomUserData(
            () => DrawSimpleRectangleNode(node, type, name, id, backgroundColour, textAlignment), name,
            id);
        return node;
    }

    public static Node CreateSimpleImageNode(string image, string name, string id)
    {
        var node = new Node(CurveFactory.CreateCircle(25, new Point())) { UserData = name };
        node.UserData = new CustomUserData(() => DrawSimpleImageNode(node, image, name, id), name, id);
        return node;
    }

    public static Cluster CreateContainerRectangleNode(string type, string name, string id,
        string backgroundColour = "#dae8fc", TextAlignment textAlignment = TextAlignment.Middle)
    {
        var node = new Cluster { BoundaryCurve = CurveFactory.CreateRectangle(200, 100, new Point()) };
        node.UserData = new CustomUserData(
            () => DrawSimpleRectangleNode(node, type, name, id, backgroundColour, textAlignment), name,
            id);
        return node;
    }

    private static string DrawSimpleRectangleNode(Node node, string type, string name, string id,
        string backgroundColour, TextAlignment textAlignment = TextAlignment.Middle)
    {
        var boundingBoxWidth = node.BoundingBox.Width;
        var boundingBoxHeight = node.BoundingBox.Height;
        var boundingBoxLeft = node.BoundingBox.Left;
        var boundingBoxTop = node.BoundingBox.Bottom;
        if (node.ClusterParent != null)
        {
            boundingBoxLeft -= node.ClusterParent.BoundingBox.Left;
            boundingBoxTop -= node.ClusterParent.BoundingBox.Bottom;
        }

        var parent = "1";

        if (node.ClusterParent is Cluster cluster)
            if (!IsRootCluster(cluster))
                parent = ((CustomUserData)cluster.UserData).Id;

        var text = name;
        if (!string.IsNullOrEmpty(type)) text += $"&lt;br/&gt;({type})";

        return
            @$"<mxCell id=""{id}"" value=""{text}"" style=""rounded=0;whiteSpace=wrap;html=1;fillColor={backgroundColour};verticalAlign={textAlignment.ToString().ToLowerInvariant()}"" vertex=""1"" parent=""{parent}"">
    <mxGeometry x=""{boundingBoxLeft}"" y=""{boundingBoxTop}"" width=""{boundingBoxWidth}"" height=""{boundingBoxHeight}"" 
    as=""geometry"" />
</mxCell>";
    }

    private static string DrawSimpleImageNode(Node node, string image, string name, string id)
    {
        var boundingBoxWidth = node.BoundingBox.Width;
        var boundingBoxHeight = node.BoundingBox.Height;
        var boundingBoxLeft = node.BoundingBox.Left;
        var boundingBoxTop = node.BoundingBox.Bottom;
        if (node.ClusterParent != null)
        {
            boundingBoxLeft -= node.ClusterParent.BoundingBox.Left;
            boundingBoxTop -= node.ClusterParent.BoundingBox.Bottom;
        }

        var parent = "1";

        if (node.ClusterParent is Cluster cluster)
            if (!IsRootCluster(cluster))
                parent = ((CustomUserData)cluster.UserData).Id;

        return
            @$"<mxCell id=""{id}"" value=""{name}"" style=""html=1;image;image={image};fontSize=12;labelPosition=bottom"" vertex=""1"" parent=""{parent}"">
    <mxGeometry x=""{boundingBoxLeft}"" y=""{boundingBoxTop}"" width=""{boundingBoxWidth}"" height=""{boundingBoxHeight}"" 
    as=""geometry"" />
</mxCell>";
    }

    private static bool IsRootCluster(Cluster node)
    {
        return node.UserData == null;
    }

    public static Edge CreateSimpleEdge(Node source, Node target, string? details, FlowEmphasis flowEmphasis)
    {
        var pattern = flowEmphasis switch
        {
            FlowEmphasis.LessImportant => Pattern.Dashed,
            _ => Pattern.Solid
        };
        
        var edge = new Edge(source, target)
        {
            UserData = new CustomUserData(
                () => DrawSimpleEdge(
                    ((CustomUserData)source.UserData).Id,
                    ((CustomUserData)target.UserData).Id,
                    details,
                    pattern),
                "Unused",
                Guid.NewGuid().ToString())
        };
        return edge;
    }

    private static string DrawSimpleEdge(string fromId, string toId, string? details, Pattern pattern)
    {
        var edgeId = Guid.NewGuid().ToString().Replace("-", "");
        var patternStyle = pattern switch
        {
            Pattern.Dashed => ";dashed=1;dashPattern=1 1;strokeColor=#82b366;",
            _ => ""
        };

        var edge = @$"<mxCell id=""{edgeId}"" 
        style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=0.5;entryDx=0;entryDy=0;entryPerimeter=0;{patternStyle}"" edge=""1"" parent=""1"" 
        source=""{fromId}"" target=""{toId}"">
            <mxGeometry relative=""1"" as=""geometry"" />
            </mxCell>
            ";
        if (string.IsNullOrEmpty(details)) return edge;

        edge +=
            @$"<mxCell id=""{edgeId}--1"" value=""{details}"" style=""edgeLabel;html=1;align=center;verticalAlign=middle;resizable=0;points=[];"" vertex=""1"" connectable=""0"" parent=""{edgeId}"">
            <mxGeometry x=""-0.5"" relative=""1"" as=""geometry"">
            <mxPoint as=""offset"" />
            </mxGeometry>
            </mxCell>";

        return edge;
    }
}