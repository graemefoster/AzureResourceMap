using System.Collections.Generic;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Diagrams;

public interface IDiagramResourceBuilder
{
    IEnumerable<Node> CreateNodes();
}