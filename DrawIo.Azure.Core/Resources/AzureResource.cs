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
    private readonly string _id = default!;
    public List<ResourceLink> Links { get; } = new();
    public virtual bool FetchFull => false;


    public string Id
    {
        get => _id;
        init
        {
            _id = value;
            InternalId = new Guid(SHA256.HashData(Encoding.UTF8.GetBytes(value))[..16]).ToString();
        }
    }

    /// <summary>
    ///     This is a deterministic guid based on the Resource Id. You cannot set it. It's worked out when you set the ID.
    /// </summary>
    public string InternalId { get; private init; } = default!;

    public string Name { get; set; } = default!;
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
    public bool ContainedByAnotherResource { get; protected internal set; }

    public virtual AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new AzureResourceNodeBuilder(this);
    }

    public virtual Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Opportunity to explode any 'new' nodes that aren't represented by ARM resources, but important to the diagram.
    ///     Example is App-Service VNet Integration. We want to show flows going through the vnet-integrated subnet.
    /// </summary>
    public virtual IEnumerable<AzureResource> DiscoverNewNodes()
    {
        yield break;
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