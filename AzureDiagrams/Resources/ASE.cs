using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class ASE : AzureResource, ICanInjectIntoASubnet
{
    public override string Image => "img/lib/azure2/app_services/App_Service_Environments.svg";

    public string[] SubnetIdsIAmInjectedInto { get; private set; } = default!;

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        SubnetIdsIAmInjectedInto = new[] { full["properties"]!["virtualNetwork"]!.Value<string>("id")! };
        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var asps = allResources.OfType<ASP>().Where(x =>
            string.Equals(Id, x.ASE, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        asps.ForEach(OwnsResource);
        base.BuildRelationships(allResources);
    }
}