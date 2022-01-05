using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Diagrams;

internal static class AzureResourceRectangleDrawer
{
    public static Node CreateSimpleRectangleNode(string name)
    {
        var node = new Node(CurveFactory.CreateRectangle(150, 75, new Point())) { UserData = name };
        node.UserData = new CustomUserData { Draw = () => DrawSimpleRectangleNode(node, name), Name = name };
        return node;
    }

    public static Cluster CreateContainerRectangleNode(string name)
    {
        var node = new Cluster { BoundaryCurve = CurveFactory.CreateRectangle(150, 75, new Point()) };
        node.UserData = new CustomUserData { Draw = () => DrawSimpleRectangleNode(node, name), Name = name };
        return node;
    }

    public static string DrawSimpleRectangleNode(Node node, string name)
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
            @$"<mxCell id=""{name}"" value=""{name}"" style=""rounded=0;whiteSpace=wrap;html=1;fillColor=#dae8fc"" vertex=""1"" parent=""{(node.ClusterParent == null ? "1" : ((CustomUserData)node.ClusterParent.UserData).Name)}"">
    <mxGeometry x=""{boundingBoxLeft}"" y=""{boundingBoxTop}"" width=""{boundingBoxWidth}"" height=""{boundingBoxHeight}"" 
    as=""geometry"" />
</mxCell>";
    }
}