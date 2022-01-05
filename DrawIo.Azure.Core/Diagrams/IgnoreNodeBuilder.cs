using System.Collections.Generic;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Diagrams;

internal class IgnoreNodeBuilder : IDiagramResourceBuilder
{
    public IEnumerable<Node> CreateNodes()
    {
        yield break;
    }
}