using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Diagrams;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class AzureResource 
{
    private readonly string _id = default!;

    /// <summary>
    ///     Settings this to true stops an icon being drawn for 'this' resource. Instead we just draw a box with the resources
    ///     that it owns.
    /// </summary>
    public virtual bool IsPureContainer => false;

    public List<ResourceLink> Links { get; } = new();
    public List<AzureResource> ContainedResources { get; } = new();

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
    public virtual string? Image { get; }

    public string Location { get; set; } = default!;

    /// <summary>
    ///     Used to indicate if another resource 'owns' this one. Example would be injecting a NIC into a Subnet.
    ///     Initial use of this flag is to push the responsibility of drawing an object to the containing resource. Not the top
    ///     level.
    /// </summary>
    public bool ContainedByAnotherResource { get; protected internal set; }

    public string Type { get; set; } = default!;

    public string[]? PrivateEndpointConnections { get; private set; }

    public bool AccessedViaPrivateEndpoint(PrivateEndpoint privateEndpoint)
    {
        return PrivateEndpointConnections?.Any(x =>
            x.Equals(privateEndpoint.Id, StringComparison.InvariantCultureIgnoreCase)) ?? false;
    }

    public virtual AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new AzureResourceNodeBuilder(this);
    }

    public virtual Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        //Common ways the platform is expressed
        PrivateEndpointConnections =
            full["properties"]!["privateEndpointConnections"]
                ?.Select(x => x["properties"]!["privateEndpoint"]!.Value<string>("id")!).ToArray() ??
            Array.Empty<string>();

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
    /// <param name="flowEmphasis"></param>
    protected internal void CreateFlowTo(AzureResource to, FlowEmphasis flowEmphasis = FlowEmphasis.Important)
    {
        CreateFlowTo(to, string.Empty, flowEmphasis);
    }

    /// <summary>
    ///     Creates a flow between two resources. Commonly visualised as a line on a graph between boxes
    /// </summary>
    /// <param name="to"></param>
    /// <param name="details"></param>
    /// <param name="flowEmphasis"></param>
    protected internal void CreateFlowTo(AzureResource to, string details, FlowEmphasis flowEmphasis = FlowEmphasis.Important)
    {
        if (IsPureContainer) throw new InvalidOperationException("You cannot create a flow to a pure container");
        var link = new ResourceLink(this, to, details, flowEmphasis);
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

public enum FlowEmphasis
{
    Important,
    LessImportant
}