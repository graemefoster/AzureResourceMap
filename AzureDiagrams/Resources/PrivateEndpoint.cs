using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AzureDiagrams.Resources.Retrievers.Extensions;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

[DebuggerDisplay("{Type}/{Name}")]
public class PrivateEndpoint : AzureResource, IAssociateWithNic, ICanInjectIntoASubnet
{
    public override string Image => "img/lib/azure2/networking/Private_Link.svg";

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
            .Where(x => x.PrivateEndpointInformation!.AccessedViaPrivateEndpoint(this))
            .ToArray();

        accessedByThisPrivateEndpoint.ForEach(x => CreateFlowTo(x.Resource, Plane.All));

        //Grab hold of the resource accessed by this. Should never be more than 1. Write a warning out if we see more though
        ResourceAccessedByMe = accessedByThisPrivateEndpoint.First().Resource;
        if (!accessedByThisPrivateEndpoint.Any()) Console.WriteLine($"WARNING: Private endpoint {Id} has no backing resource. Be sure to include its resource group.");
        if (accessedByThisPrivateEndpoint.Length > 1) Console.WriteLine($"WARNING: Private endpoint {Id} has no backing resource.");

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