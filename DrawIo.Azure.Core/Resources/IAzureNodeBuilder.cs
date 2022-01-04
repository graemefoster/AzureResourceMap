using System.Collections.Generic;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Resources;

internal interface IAzureNodeBuilder
{
    IEnumerable<Node> CreateNodes(AzureResource resource);
}