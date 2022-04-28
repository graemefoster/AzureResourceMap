using Microsoft.Msagl.Core.Layout;

namespace AzureDiagramGenerator.DrawIo;

public class CustomUserData
{
    public CustomUserData(Func<string> drawNode, string name, string id)
    {
        DrawNode = drawNode;
        Name = name;
        Id = id;
    }

    public CustomUserData(Func<Edge, string> drawEdge, string name, string id)
    {
        DrawEdge = drawEdge;
        Name = name;
        Id = id;
    }

    public Func<string> DrawNode { get; init; }
    public Func<Edge, string> DrawEdge { get; init; }
    public string Name { get; init; }
    public string Id { get; init; }

    public void DrawnAt(double x, double y)
    {
        DrawnAtX = x;
        DrawnAtY = y;
    }

    public double DrawnAtY { get; set; }

    public double DrawnAtX { get; set; }
}