using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AzureDiagrams.Resources.Retrievers.Extensions;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

[DebuggerDisplay("{Type}/{Name}")]
public class AzureResource
{
    private readonly string _id = default!;
    private readonly string? _location = default!;

    /// <summary>
    ///     TODO make this a diagram construct. It is used to mark a resource that is rendered as a container of other resource, with no icon for itself.
    /// </summary>
    public virtual bool IsPureContainer => false;

    public List<ResourceLink> Links { get; } = new();
    public List<AzureResource> ContainedResources { get; } = new();
    public IEnumerable<IResourceExtension> Extensions { get; internal set; } = Array.Empty<IResourceExtension>();

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
    public virtual string Image { get; } = default!;
    public virtual string? Fill { get; }

    public string? Location
    {
        get => _location;
        init => _location = value?.Replace(" ", "").ToLowerInvariant();
    }

    public string ManagedBy { get; set; } = default!;

    /// <summary>
    ///     Used to indicate if another resource 'owns' this one. Example would be injecting a NIC into a Subnet.
    ///     Initial use of this flag is to push the responsibility of drawing an object to the containing resource. Not the top
    ///     level.
    /// </summary>
    public bool ContainedByAnotherResource { get; protected internal set; }

    public string? Type { get; set; } = default!;

    public virtual Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Opportunity to explode any 'new' nodes that aren't represented by ARM resources, but important to the diagram.
    ///     Example is App-Service VNet Integration. We want to show flows going through the vnet-integrated subnet.
    /// </summary>
    /// <param name="azureResources"></param>
    public virtual IEnumerable<AzureResource> DiscoverNewNodes(List<AzureResource> azureResources)
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
        Extensions.ForEach(x => x.BuildRelationships(this, allResources));
        ;
    }

    /// <summary>
    ///     Creates a flow between two resources. Commonly visualised as a line on a graph between boxes
    /// </summary>
    /// <param name="to"></param>
    /// <param name="plane"></param>
    protected internal void CreateFlowTo(AzureResource to, Plane plane)
    {
        CreateFlowTo(to, string.Empty, plane);
    }

    /// <summary>
    ///     Creates a flow between two resources. Commonly visualised as a line on a graph between boxes
    /// </summary>
    /// <param name="to"></param>
    /// <param name="details"></param>
    /// <param name="plane"></param>
    protected internal void CreateFlowTo(AzureResource to, string details,
        Plane plane)
    {
        if (IsPureContainer) throw new InvalidOperationException("You cannot create a flow to a pure container");

        if (Links.Any(x => x.To == to))
        {
            return;
        }

        var link = new ResourceLink(this, to, details, plane);
        Links.Add(link);
    }

    /// <summary>
    ///     Containing a resource will cause anything without a 'specific' drawer to be rendered as a container, with all
    ///     contained resources inside.
    ///     Also sets the ContainedByAnotherResource flag to tell the drawer that something else will draw it
    /// </summary>
    /// <param name="contained"></param>
    protected void OwnsResource(AzureResource contained)
    {
        ContainedResources.Add(contained);
        contained.ContainedByAnotherResource = true;
    }
    
}