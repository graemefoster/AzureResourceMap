using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Diagrams;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class AzureResource
{
    private string _id;
    public List<ResourceLink> Links { get; } = new();

    public virtual bool FetchFull => false;

    public string Id
    {
        get => _id;
        set
        {
            _id = value;
            InternalId = new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(value))[..16]).ToString();
        }
    }

    public string InternalId { get; private set; }

    public string Name { get; set; }
    public virtual string Image { get; protected set; }
    public string Type { get; set; }
    public string Location { get; set; }
    public virtual string ApiVersion => "2020-11-01";

    public virtual HashSet<(HttpMethod, string)> AdditionalResources => new();

    /// <summary>
    ///     Used to indicate if another resource 'owns' this one. Example would be injecting a NIC into a Subnet.
    ///     Initial use of this flag is to push the responsibility of drawing an object to the containing resource. Not the top
    ///     level.
    /// </summary>
    public bool ContainedByAnotherResource { get; protected internal set; } = false;

    public virtual AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new AzureResourceNodeBuilder(this);
    }

    public virtual Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Override this to build derived relationships between nodes.
    ///     An example would be using metadata to add private endpoints / NICs into subnets.
    /// </summary>
    /// <param name="allResources"></param>
    public virtual void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
    }

    /// <summary>
    ///     Creates a flow between two resources. Commonly visualised as a line on a graph between boxes
    /// </summary>
    /// <param name="to"></param>
    protected internal void CreateFlowTo(AzureResource to)
    {
        var link = new ResourceLink(this, to);
        Links.Add(link);
    }
}