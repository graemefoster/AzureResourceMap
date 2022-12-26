using System.Text;
using AzureDiagramGenerator.DrawIo;
using AzureDiagramGenerator.DrawIo.DiagramAdjustors;
using AzureDiagrams.Resources;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;

namespace AzureDiagramGenerator;

public static class DrawIoDiagramGenerator
{
    public static Task<string> DrawDiagram(
        AzureResource[] resources,
        bool condensed,
        bool showDiagnosticsFlows,
        bool showInferredFlows,
        bool showRuntimeFlows,
        bool showIdentityFlows
    )
    {
        var graph = new GeometryGraph();

        var planes = showDiagnosticsFlows ? Plane.Diagnostics : Plane.None;
        planes |= showInferredFlows ? Plane.Inferred : Plane.None;
        planes |= showRuntimeFlows ? Plane.Runtime : Plane.None;
        planes |= showIdentityFlows ? Plane.Identity : Plane.None;
        var adjustor = (IDiagramAdjustor)new VisiblePlanesDiagramAdjustor(planes);
        adjustor = condensed ? new CondensedDiagramAdjustor(adjustor, resources) : adjustor;

        var nodeBuilders = resources.ToDictionary(x => x, x => AzureResourceNodeBuilder.CreateNodeBuilder(x, adjustor));
        adjustor.PostProcess(nodeBuilders);
        var nodes = nodeBuilders.SelectMany(x => x.Value.CreateNodes(nodeBuilders, adjustor)).ToArray();
        var nodesGroupedByResource = nodes.GroupBy(x => x.Item1, x => x.Item2);
        var nodesDictionary = nodesGroupedByResource.ToDictionary(x => x.Key, x => x.ToArray());

        var edges = nodeBuilders.Values.SelectMany(x => x.CreateEdges(nodesDictionary, adjustor)).ToArray();

        nodesDictionary.SelectMany(x => x.Value).ForEach(n =>
        {
            if (n is Cluster)
            {
                if (n.ClusterParent == null)
                    graph.RootCluster.AddChild(n);
            }
            else
            {
                graph.Nodes.Add(n);
            }
        });
        edges.ForEach(graph.Edges.Add);

        var sb = new StringBuilder();

        var routingSettings = new EdgeRoutingSettings
        {
            Padding = 5,
            BendPenalty = 10,
            UseObstacleRectangles = true,
            EdgeRoutingMode = EdgeRoutingMode.Rectilinear
        };

        var settings = new SugiyamaLayoutSettings
        {
            PackingAspectRatio = 3,
            PackingMethod = PackingMethod.Compact,
            LayerSeparation = 25,
            EdgeRoutingSettings = routingSettings,
            LiftCrossEdges = true,
            NodeSeparation = 25,
            ClusterMargin = 50,
        };

        LayoutHelpers.CalculateLayout(graph, settings, null);

        var msGraph = @$"<mxGraphModel>
	<root>
		<mxCell id=""0"" />
		<mxCell id=""1"" parent=""0"" />
{string.Join(Environment.NewLine, graph.GetFlattenedNodesAndClusters().Select(v => ((CustomUserData)v.UserData).DrawNode!()))}
{string.Join(Environment.NewLine, graph.Edges.Select(v => ((CustomUserData)v.UserData).DrawEdge!(v)))}
{sb}
	</root>
</mxGraphModel>";

        return Task.FromResult(msGraph);
    }
}