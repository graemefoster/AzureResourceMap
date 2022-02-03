using Microsoft.Msagl.Core.Layout;

namespace AzureDiagramGenerator.DrawIo;

public interface IDiagramResourceBuilder
{
    IEnumerable<Node> CreateNodes();
}