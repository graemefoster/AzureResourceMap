using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class PrivateEndpoint : AzureResource, IAssociateWithNic
{
    public override bool FetchFull => true;
    public override string Image => "img/lib/azure2/networking/Private_Link.svg";

    public string[] Nics { get; private set; }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources
            .OfType<ICanBeExposedByPrivateEndpoints>().Where(x => x.AccessedViaPrivateEndpoint(this))
            .ForEach(x => base.CreateFlowTo((AzureResource)x));
    }

    public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
    {
        Nics = jObject["properties"]!["networkInterfaces"]!.Select(x => x.Value<string>("id")!).ToArray();

        return Task.CompletedTask;
    }
}