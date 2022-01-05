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
    private readonly List<ResourceLink> _links = new();
    private string _id;

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

    public virtual HashSet<(HttpMethod, string)> AdditionalResources => new();

    public virtual IDiagramResourceBuilder CreateNodeBuilder()
    {
        return new AzureResourceNodeBuilder(this);
    }

    public virtual Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Override this to build derived relationships between nodes.
    /// </summary>
    /// <param name="allResources"></param>
    /// <param name="graph"></param>
    public virtual void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
    }

    protected void Link(AzureResource to)
    {
        var link = new ResourceLink(this, to);
        _links.Add(link);
    }
}

internal class ResourceLink
{
    public ResourceLink(AzureResource from, AzureResource to)
    {
        From = from;
        To = to;
    }

    public AzureResource From { get; }
    public AzureResource To { get; }
}