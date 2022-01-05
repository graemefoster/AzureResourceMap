using System;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Diagrams;

internal static class AzureResourceDrawer
{
    public static Node CreateSimpleRectangleNode(string type, string name, string id, string backgroundColour = "#dae8fc")
    {
        var node = new Node(CurveFactory.CreateRectangle(150, 75, new Point())) { UserData = name };
        node.UserData = new CustomUserData(() => DrawSimpleRectangleNode(node, type, name, id, backgroundColour), name, id);
        return node;
    }

    public static Node CreateSimpleImageNode(string image, string name, string id)
    {
        var node = new Node(CurveFactory.CreateCircle(25, new Point())) { UserData = name };
        node.UserData = new CustomUserData(() => DrawSimpleImageNode(node, image, name, id), name, id);
        return node;
    }

    public static Cluster CreateContainerRectangleNode(string type, string name, string id, string backgroundColour = "#dae8fc")
    {
        var node = new Cluster { BoundaryCurve = CurveFactory.CreateRectangle(150, 75, new Point()) };
        node.UserData = new CustomUserData(() => DrawSimpleRectangleNode(node, type, name, id, backgroundColour), name, id);
        return node;
    }

    private static string DrawSimpleRectangleNode(Node node, string type, string name, string id, string backgroundColour)
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

        return
            @$"<mxCell id=""{id}"" value=""{name}&lt;br/&gt;({type})"" style=""rounded=0;whiteSpace=wrap;html=1;fillColor={backgroundColour}"" vertex=""1"" parent=""{(node.ClusterParent == null ? "1" : ((CustomUserData)node.ClusterParent.UserData).Id)}"">
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

        return
            @$"<mxCell id=""{id}"" value=""{name}"" style=""html=1;image;image={image};fontSize=12;labelPosition=bottom"" vertex=""1"" parent=""{(node.ClusterParent == null ? "1" : ((CustomUserData)node.ClusterParent.UserData).Id)}"">
    <mxGeometry x=""{boundingBoxLeft}"" y=""{boundingBoxTop}"" width=""{boundingBoxWidth}"" height=""{boundingBoxHeight}"" 
    as=""geometry"" />
</mxCell>";
    }

    public static Edge CreateSimpleEdge(Node source, Node target)
    {
        var edge = new Edge(source, target)
        {
            UserData = new CustomUserData(
                () => DrawSimpleEdge(((CustomUserData)source.UserData).Id, ((CustomUserData)target.UserData).Id),
                "Unused", Guid.NewGuid().ToString())
        };
        return edge;
    }

    private static string DrawSimpleEdge(string fromId, string toId)
    {
        return @$"<mxCell id=""{Guid.NewGuid().ToString().Replace("-", "")}"" 
        style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=0.5;entryDx=0;entryDy=0;entryPerimeter=0;"" edge=""1"" parent=""1"" 
        source=""{fromId}"" target=""{toId}"">
            <mxGeometry relative=""1"" as=""geometry"" />
            </mxCell>
            ";
    }
}