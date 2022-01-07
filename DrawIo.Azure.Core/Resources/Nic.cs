using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Diagrams;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class Nic : AzureResource, ICanInjectIntoASubnet, ICanExposePublicIPAddresses
{
    public override string Image => "img/lib/azure2/networking/Network_Interfaces.svg";
    public string[] PublicIpAddresses { get; set; } = default!;

    private string[] NetworkAttachments { get; set; } = default!;

    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new AzureResourceNodeBuilder(this);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<PrivateEndpoint>().Where(x => x.Nics.Contains(Id)).ForEach(CreateFlowTo);
        allResources.OfType<VM>().Where(x => x.Nics.Contains(Id)).ForEach(CreateFlowTo);
    }

    public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
    {
        PublicIpAddresses = jObject["properties"]!["ipConfigurations"]!
            .Select(x =>
                x["properties"]!["publicIPAddress"] != null
                    ? x["properties"]!["publicIPAddress"]!.Value<string>("id")!.ToLowerInvariant()
                    : null)
            .Where(x => x != null)
            .ToArray()!;

        NetworkAttachments = jObject["properties"]!["ipConfigurations"]!
            .Select(x =>
                x["properties"]!["subnet"] != null
                    ? x["properties"]!["subnet"]!.Value<string>("id")!.ToLowerInvariant()
                    : null)
            .Where(x => x != null)
            .Select(x => x!)
            .ToArray();
        
        return Task.CompletedTask;
    }

    public void AssignNsg(NSG nsg)
    {
        OwnsResource(nsg);
    }

    public string[] SubnetIdsIAmInjectedInto => NetworkAttachments;
}