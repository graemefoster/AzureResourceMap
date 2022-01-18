using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Resources.Retrievers.Extensions;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class PrivateEndpoint : AzureResource, IAssociateWithNic, ICanInjectIntoASubnet
{
    public override string Image => "img/lib/azure2/networking/Private_Link.svg";
    public string[] CustomHostNames { get; private set; } = default!;

    public string[] Nics { get; private set; } = default!;

    public string[] SubnetIdsIAmInjectedInto { get; private set; } = default!;

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources
            .Select(x => new { Resource = x, PrivateEndpointInformation = x.Extensions.OfType<PrivateEndpointExtensions>().SingleOrDefault()})
            .Where(x => x.PrivateEndpointInformation != null)
            .Where(x => x.PrivateEndpointInformation!.AccessedViaPrivateEndpoint(this))
            .ForEach(x => CreateFlowTo(x.Resource));
        base.BuildRelationships(allResources);
    }

    public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
    {
        Nics = jObject["properties"]!["networkInterfaces"]!.Select(x => x.Value<string>("id")!).ToArray();
        CustomHostNames = jObject["properties"]!["customDnsConfigs"]!.Select(x => x.Value<string>("fqdn")!).ToArray();
        SubnetIdsIAmInjectedInto = new[] { jObject["properties"]!["subnet"]!.Value<string>("id")! };

        return Task.CompletedTask;
    }
}