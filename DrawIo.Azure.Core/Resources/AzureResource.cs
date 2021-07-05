using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<Edge> _edges = new();
        public virtual void Enrich(JObject full)
        {
        }

        public virtual IEnumerable<string> ToDrawIo()
        {
            var cellStyle = string.IsNullOrEmpty(Image)
                ? "rounded=0;whiteSpace=wrap;html=1;"
                : $"html=1;image;image={Image};fontSize=12;labelPosition=bottom";

            return new[]
            {
                @$"<mxCell id=""{Id}"" value=""{Name}"" style=""{cellStyle}"" vertex=""1"" parent=""1"">
                <mxGeometry x=""{Node.Center.X}"" y=""{Node.Center.Y}"" width=""{Node.Width}"" height=""{Node.Height}"" as=""geometry"" />
                </mxCell>"
            }.Union(_edges.Select(x =>
            {

                var argUserData = ((AzureResource) x.UserData);
                return @$"
<mxCell id=""{Guid.NewGuid().ToString().Replace("-", "")}"" 
        style=""edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;entryX=0.5;entryY=0.5;entryDx=0;entryDy=0;entryPerimeter=0;"" edge=""1"" parent=""1"" 
        source=""{Id}"" target=""{argUserData.Id}"">
                        <mxGeometry relative=""1"" as=""geometry"" />
                    </mxCell>
";
            }));
        }

        public virtual void Link(IEnumerable<AzureResource> allResources, GeometryGraph graph)
        {
        }

        public void Link(AzureResource to, GeometryGraph graph)
        {
            var edge = new Edge(Node, to.Node) {UserData = to};
            graph.Edges.Add(edge);
            _edges.Add(edge);
        }
    }

    internal interface IContainResources
    {
        void Group(GeometryGraph graph, IEnumerable<AzureResource> allResources);
    }
}