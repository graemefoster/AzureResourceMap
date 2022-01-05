using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Resources.Diagrams;
using Microsoft.Msagl.Core.Layout;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class AzureResource
    {
        internal class AzureResourceNodeBuilder : IAzureNodeBuilder
        {
            public IEnumerable<Node> CreateNodes(AzureResource resource)
            {
                yield return AzureResourceRectangleDrawer.CreateSimpleRectangleNode(resource.Name);
            }
        }

        public virtual IAzureNodeBuilder CreateNodeBuilder()
        {
            return new AzureResourceNodeBuilder();
        }

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
        private readonly List<Edge> _edges = new();
        private string _id;

        public virtual HashSet<(HttpMethod, string)> AdditionalResources => new();

        public virtual Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
        {
            return Task.CompletedTask;
        }

        // public virtual void Link(IEnumerable<AzureResource> allResources, GeometryGraph graph)
        // {
        // }
        //
        // public void Link(AzureResource to, GeometryGraph graph)
        // {
        //     var edge = new Edge(Node, to.Node) { UserData = to };
        //     graph.Edges.Add(edge);
        //     _edges.Add(edge);
        // }
        
    }
    
}