using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureDiagrams.Resources.Retrievers.Extensions;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class PrivateEndpoint : AzureResource, IAssociateWithNic, ICanInjectIntoASubnet
{
    public override string Image => ResourceAccessedByMe switch
    {
        null => "img/lib/azure2/networking/Private_Link.svg",
        _ => ResourceAccessedByMe!.Image!
    };

    public string[] CustomHostNames { get; private set; } = default!;

    public AzureResource? ResourceAccessedByMe { get; set; }

    public string[] Nics { get; private set; } = default!;

    public string[] SubnetIdsIAmInjectedInto { get; private set; } = default!;

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var accessedByThisPrivateEndpoint = allResources
            .Select(x => new
            {
                Resource = x,
                PrivateEndpointInformation = x.Extensions.OfType<PrivateEndpointExtensions>().SingleOrDefault()
            })
            .Where(x => x.PrivateEndpointInformation != null)
            .Where(x => x.PrivateEndpointInformation!.AccessedViaPrivateEndpoint(this));

        accessedByThisPrivateEndpoint
            .ForEach(x => CreateFlowTo(x.Resource));

        //Grab hold of the resource accessed by this (if it's only a single one).
        ResourceAccessedByMe = accessedByThisPrivateEndpoint.Count() == 1
            ? accessedByThisPrivateEndpoint.Single().Resource
            : null;

        base.BuildRelationships(allResources);
    }

    public override Task Enrich(JObject jObject, Dictionary<string, JObject?> additionalResources)
    {
        Nics = jObject["properties"]!["networkInterfaces"]!.Select(x => x.Value<string>("id")!).ToArray();
        CustomHostNames = jObject["properties"]!["customDnsConfigs"]!.Select(x => x.Value<string>("fqdn")!).ToArray();
        SubnetIdsIAmInjectedInto = new[] { jObject["properties"]!["subnet"]!.Value<string>("id")! };

        return Task.CompletedTask;
    }
}