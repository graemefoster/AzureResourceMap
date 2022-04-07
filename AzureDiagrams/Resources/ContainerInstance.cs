using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class ContainerInstance : AzureResource, ICanInjectIntoASubnet
{
    private RelationshipHelper _hostNameDiscoverer = default!;
    private string? _networkProfile = default!;
    public override string Image => "img/lib/azure2/containers/Container_Instances.svg";

    public string[] SubnetIdsIAmInjectedInto { get; private set; } = default!;


    public override async Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        await base.Enrich(full, additionalResources);

        _networkProfile = full["properties"]!["networkProfile"]?.Value<string>("id");

        var imageSource = full["properties"]!["containers"]
            ?.Select(x => x["properties"]!.Value<string>("image")?.Split('/')[0]);

        var properties = full["properties"]!["containers"]?.SelectMany(x => x["properties"]!["environmentVariables"]!
            .Select(x => x.Value<string>("value"))) ?? Enumerable.Empty<string>();

        if (imageSource != null)
            //technically not a URL but the relationship helper will sort that out for us: 
            properties = properties.Concat(imageSource.Select(x => $"https://{x}")).Distinct();

        _hostNameDiscoverer =
            new RelationshipHelper(properties.Where(x => x != null).Select(x => (object)x!).ToArray());

        _hostNameDiscoverer.Discover();
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        base.BuildRelationships(allResources);
        _hostNameDiscoverer.BuildRelationships(this, allResources);
    }

    /// <summary>
    ///     Using this as a hook to find the network profile that tells me which subnet I'm injected into
    /// </summary>
    /// <param name="azureResources"></param>
    /// <returns></returns>
    public override IEnumerable<AzureResource> DiscoverNewNodes(List<AzureResource> azureResources)
    {
        if (_networkProfile != null)
        {
            SubnetIdsIAmInjectedInto = azureResources.OfType<NetworkProfile>()
                .SingleOrDefault(x => x.Id.Equals(_networkProfile!, StringComparison.InvariantCultureIgnoreCase))?.SubnetIds ?? Array.Empty<string>();
        }

        return base.DiscoverNewNodes(azureResources);
    }
}