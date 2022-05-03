using System.Security.Cryptography;
using System.Text;
using AzureDiagrams.Resources;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;

namespace AzureDiagramGenerator.DrawIo;

internal static class AzureResourceDrawer
{
    public static Node CreateSimpleRectangleNode(string type, string name, string id,
        string? backgroundColour = null, TextAlignment textAlignment = TextAlignment.Middle)
    {
        var node = new Node(CurveFactory.CreateRectangle(150, 75, new Point())) { UserData = name };
        node.UserData = new CustomUserData(
            () => DrawSimpleRectangleNode(node, type, name, id, backgroundColour ?? "#dae8fc", textAlignment), name,
            id);
        return node;
    }

    public static Node CreateTextNode(string text, string id)
    {
        var node = new Node(CurveFactory.CreateRectangle(150, 75, new Point())) { UserData = text };
        node.UserData = new CustomUserData(
            () => DrawSimpleTextNode(node, text, id),
            id,
            id);
        return node;
    }

    public static Node CreateSimpleImageNode(string image, string name, string id)
    {
        var node = new Node(CurveFactory.CreateCircle(25, new Point())) { UserData = name };
        node.UserData = new CustomUserData(() => DrawSimpleImageNode(node, image, name, id), name, id);
        return node;
    }

    public static Cluster CreateContainerRectangleNode(
        string type,
        string name,
        string id,
        string? backgroundColour = null,
        TextAlignment textAlignment = TextAlignment.Middle,
        params string[] images)
    {
        var node = new Cluster { BoundaryCurve = CurveFactory.CreateRectangle(200, 100, new Point()) };
        node.UserData = new CustomUserData(
            () => DrawSimpleRectangleNode(node, type, name, id, backgroundColour ?? "#FFE6CC", textAlignment, images),
            name,
            id);
        return node;
    }

    private static string DrawSimpleRectangleNode(Node node, string type, string name, string id,
        string backgroundColour, TextAlignment textAlignment = TextAlignment.Middle, params string[] images)
    {
        var boundingBoxWidth = node.BoundingBox.Width;
        var boundingBoxHeight = node.BoundingBox.Height;
        var boundingBoxLeft = node.BoundingBox.Left;
        var boundingBoxTop = node.BoundingBox.Bottom;
        if (node.ClusterParent != null && node.ClusterParent.ClusterParent != null)
        {
            boundingBoxLeft -= node.ClusterParent.BoundingBox.Left;
            boundingBoxTop -= node.ClusterParent.BoundingBox.Bottom;
        }

        var parent = "1";

        if (node.ClusterParent != null)
            if (!IsRootCluster(node.ClusterParent))
                parent = ((CustomUserData)node.ClusterParent.UserData).Id;

        var text = name;
        if (images.Length == 0 && !string.IsNullOrEmpty(type)) text += $"&lt;br/&gt;({type})";

        var container =
            @$"<mxCell id=""{id}"" value=""{text}"" style=""rounded=0;whiteSpace=wrap;html=1;fillColor={backgroundColour};verticalAlign={textAlignment.ToString().ToLowerInvariant()}"" vertex=""1"" parent=""{parent}"">
    <mxGeometry x=""{boundingBoxLeft}"" y=""{boundingBoxTop}"" width=""{boundingBoxWidth}"" height=""{boundingBoxHeight}"" 
    as=""geometry"" />
</mxCell>";

        const int imageSize = 30;
        images.ForEach((idx, image) =>
            {
                container += Environment.NewLine +
                             @$"<mxCell id=""{id}.image.{idx}"" style=""html=1;image;image={image};fontSize=12;labelPosition=bottom"" vertex=""1"" parent=""{id}"">
    <mxGeometry x=""{boundingBoxWidth - ((idx + 1) * (imageSize + 10))}"" y=""{boundingBoxHeight - imageSize - 10}"" width=""{imageSize}"" height=""{imageSize}"" 
    as=""geometry"" />
</mxCell>";
            }
        );

        return container;
    }

    private static string DrawSimpleImageNode(Node node, string image, string name, string id)
    {
        var boundingBoxWidth = node.BoundingBox.Width;
        var boundingBoxHeight = node.BoundingBox.Height;
        var boundingBoxLeft = node.BoundingBox.Left;
        var boundingBoxTop = node.BoundingBox.Bottom;
        if (node.ClusterParent != null && node.ClusterParent.ClusterParent != null)
        {
            boundingBoxLeft -= node.ClusterParent.BoundingBox.Left;
            boundingBoxTop -= node.ClusterParent.BoundingBox.Bottom;
        }

        var parent = "1";

        if (node.ClusterParent != null)
            if (!IsRootCluster(node.ClusterParent))
                parent = ((CustomUserData)node.ClusterParent.UserData).Id;

        return
            @$"<mxCell id=""{id}"" value=""{name}"" style=""html=1;image;image={image};fontSize=12;labelPosition=bottom"" vertex=""1"" parent=""{parent}"">
    <mxGeometry x=""{boundingBoxLeft}"" y=""{boundingBoxTop}"" width=""{boundingBoxWidth}"" height=""{boundingBoxHeight}"" 
    as=""geometry"" />
</mxCell>";
    }

    private static string DrawSimpleTextNode(Node node, string text, string id)
    {
        var boundingBoxWidth = node.BoundingBox.Width;
        var boundingBoxHeight = node.BoundingBox.Height;
        var boundingBoxLeft = node.BoundingBox.Left;
        var boundingBoxTop = node.BoundingBox.Bottom;
        if (node.ClusterParent != null && node.ClusterParent.ClusterParent != null)
        {
            boundingBoxLeft -= node.ClusterParent.BoundingBox.Left;
            boundingBoxTop -= node.ClusterParent.BoundingBox.Bottom;
        }

        var parent = "1";

        if (node.ClusterParent != null)
            if (!IsRootCluster(node.ClusterParent))
                parent = ((CustomUserData)node.ClusterParent.UserData).Id;

        return
            @$"<mxCell id=""{id}"" value=""{text}"" style=""text;align=left;fontSize=12;verticalAlign=middle;resizable=0;points=[];autosize=1;strokeColor=none;fillColor=none;"" vertex=""1"" parent=""{parent}"">
    <mxGeometry x=""{boundingBoxLeft}"" y=""{boundingBoxTop}"" width=""{boundingBoxWidth}"" height=""{boundingBoxHeight}"" as=""geometry"" />
</mxCell>";
    }

    private static bool IsRootCluster(Cluster node)
    {
        return node.UserData == null;
    }

    public static Edge CreateSimpleEdge(AzureResource originalFrom, AzureResource originalTo, Node source, Node target,
        string? details, Plane plane, bool isLinkVisible)
    {
        //Use the original resources as condensing a diagram may cause multiple links to share the same from / to / details.
        var edgeId = new Guid(
            SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes($"{originalFrom.Id}-{originalTo.Id}-{details}"))[..16]);

        var pattern = plane switch
        {
            Plane.Diagnostics => Pattern.Dashed,
            Plane.Identity => Pattern.Dashed,
            _ => Pattern.Solid
        };

        var edge = new Edge(source, target)
        {
            UserData = new CustomUserData(
                e => DrawSimpleEdge(
                    e,
                    edgeId,
                    ((CustomUserData)source.UserData).Id,
                    ((CustomUserData)target.UserData).Id,
                    details,
                    pattern,
                    isLinkVisible),
                "Unused",
                Guid.NewGuid().ToString())
        };
        return edge;
    }

    private static string DrawSimpleEdge(Edge edge, Guid edgeId, string fromId, string toId, string? details,
        Pattern pattern,
        bool isLinkVisible)
    {
        if (!isLinkVisible) return string.Empty;

        var colour = pattern switch
        {
            Pattern.Dashed => "#4D9900",
            _ => "#000000"
        };

        var patternStyle = pattern switch
        {
            Pattern.Dashed => $";dashed=1;dashPattern=1 1;strokeColor={colour};strokeWidth=2;",
            _ => ""
        };

        var points = string.Empty;
        if (edge.EdgeGeometry.Curve is Curve curve)
        {
            points = @$"<mxPoint x=""{curve.Start.X}"" y=""{curve.Start.Y}"" as=""sourcePoint"" />
<mxPoint x=""{curve.End.X}"" y=""{curve.End.Y}"" as=""targetPoint"" />
<Array as=""points"">
    {string.Join(Environment.NewLine, curve.Segments.Select(x => $"<mxPoint x=\"{x.Start.X}\" y=\"{x.Start.Y}\" />"))}
</Array>";

            patternStyle += "rounded=1;orthogonalLoop=1;";
        }
        else if (edge.EdgeGeometry.Curve is LineSegment line)
        {
            points = @$"<Array as=""points"">
    <mxPoint x=""{line.Start.X}"" y=""{line.Start.Y}"" as=""sourcePoint"" />
    <mxPoint x=""{line.End.X}"" y=""{line.End.Y}"" as=""targetPoint"" />
</Array>";
            patternStyle += "rounded=0;orthogonalLoop=1;";
        }
        else
        {
            patternStyle += "edgeStyle=orthogonalEdgeStyle;orthogonalLoop=1;rounded=1";
        }

        var drawIoEdge = @$"<mxCell id=""{edgeId}"" 
        style=""jettySize=auto;html=1;entryX=0.5;entryY=0.5;entryDx=0;entryDy=0;entryPerimeter=0;{patternStyle};"" edge=""1"" parent=""1"" 
        source=""{fromId}"" target=""{toId}"">
            <mxGeometry relative=""1"" as=""geometry"">{points}</mxGeometry>
            </mxCell>
            ";
        if (string.IsNullOrEmpty(details)) return drawIoEdge;

        drawIoEdge +=
            @$"<mxCell id=""{edgeId}--1"" value=""{details}"" style=""edgeLabel;html=1;align=center;verticalAlign=middle;resizable=0;points=[];"" vertex=""1"" connectable=""0"" parent=""{edgeId}"">
            <mxGeometry x=""-0.5"" relative=""1"" as=""geometry"">
            <mxPoint as=""offset"" />
            </mxGeometry>
            </mxCell>";

        return drawIoEdge;
    }
}