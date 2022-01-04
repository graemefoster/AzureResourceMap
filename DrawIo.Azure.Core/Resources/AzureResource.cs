using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Core.Layout;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class AzureResource
    {
        public virtual bool IsSpecific => false;
        public virtual bool FetchFull => false;

        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                InternalId = new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(value))[..16]);
            }
        }

        public Guid InternalId { get; private set; }

        public string Name { get; set; }
        public virtual string Image { get; protected set; }
        public string Type { get; set; }
        public string Location { get; set; }
        public virtual string ApiVersion => "2020-11-01";
        public Node Node { get; set; }
        private readonly List<Edge> _edges = new();
        private string _id;

        public virtual HashSet<(HttpMethod, string)> AdditionalResources => new();

        public virtual Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
        {
            return Task.CompletedTask;
        }

        public virtual IEnumerable<string> ToDrawIo()
        {
            var cellStyle = string.IsNullOrEmpty(Image)
                ? "rounded=0;whiteSpace=wrap;html=1;"
                : $"html=1;image;image={Image};fontSize=12;labelPosition=bottom";

            var boundingBoxWidth = Node.BoundingBox.Width;
            var boundingBoxHeight = Node.BoundingBox.Height;
            var boundingBoxLeft = Node.BoundingBox.Left;
            var boundingBoxTop = Node.BoundingBox.Bottom;

            return new[]
            {
                @$"<mxCell id=""{InternalId}"" value=""{Name}"" style=""{cellStyle}"" vertex=""1"" parent=""1"">
                <mxGeometry x=""{boundingBoxLeft}"" y=""{boundingBoxTop}"" width=""{boundingBoxWidth}"" height=""{boundingBoxHeight}"" as=""geometry"" />
                </mxCell>"
            }.Union(_edges.Select(x =>
            {
                var argUserData = ((AzureResource)x.UserData);
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
            var edge = new Edge(Node, to.Node) { UserData = to };
            graph.Edges.Add(edge);
            _edges.Add(edge);
        }
    }
}