using System;
using System.Collections.Generic;
using Microsoft.Msagl.Core.Layout;
using Newtonsoft.Json.Linq;
using Edge = Microsoft.Msagl.Core.Layout.Edge;
using Node = Microsoft.Msagl.Core.Layout.Node;

namespace DrawIo.Azure.Core.Resources
{
    class AzureResource
    {
        public virtual bool IsSpecific => false;
        public virtual bool FetchFull => false;
        public string Id { get; set; }
        public string Name { get; set; }
        public virtual string Image { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
        public virtual string ApiVersion => "2020-11-01";
        public Node Node { get; set; }

        public virtual void Enrich(JObject full)
        {
        }

        public virtual IEnumerable<string> ToDrawIo(int x, int y)
        {
            var cellStyle = string.IsNullOrEmpty(Image)
                ? "rounded=0;whiteSpace=wrap;html=1;"
                : $"html=1;image;image={Image};fontSize=12;labelPosition=bottom";
            return new[]
            {
                @$"<mxCell id=""{Id}"" value=""{Name}"" style=""{cellStyle}"" vertex=""1"" parent=""1"">
                <mxGeometry x=""{Node.Center.X}"" y=""{Node.Center.Y}"" width=""{Node.Width}"" height=""{Node.Height}"" as=""geometry"" />
                </mxCell>"
            };
        }

        public virtual IEnumerable<string> Link(IEnumerable<AzureResource> allResources, GeometryGraph graph)
        {
            return Array.Empty<string>();
        }

        public string Link(AzureResource to, GeometryGraph graph)
        {
            graph.Edges.Add(new Edge(Node, to.Node) { Length = 200});
            return $@"
<mxCell id=""{Guid.NewGuid().ToString().Replace("-", "")}"" style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=-0.047;entryY=0.476;entryDx=0;entryDy=0;entryPerimeter=0;"" edge=""1"" parent=""1"" source=""{Id}"" target=""{to.Id}"">
    <mxGeometry relative=""1"" as=""geometry"" />
</mxCell>";
        }
    }

    internal interface IContainResources
    {
        void Group(GeometryGraph graph, IEnumerable<AzureResource> allResources);
    }
}