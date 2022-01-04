using System.Collections.Generic;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Resources;

internal interface IContainResources
{
    void Group(GeometryGraph graph, IEnumerable<AzureResource> allResources);
}