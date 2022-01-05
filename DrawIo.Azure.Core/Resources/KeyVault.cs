using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class KeyVault : AzureResource, ICanBeExposedByPrivateEndpoints
{
    public override bool FetchFull => true;
    public override string Image => "img/lib/azure2/security/Key_Vaults.svg";
    public override string ApiVersion => "2019-09-01";
    public string[] PrivateEndpoints { get; set; }

    public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
    {
        PrivateEndpoints =
            jObject["properties"]!["privateEndpointConnections"]!
                .Select(x => x["properties"]!["privateEndpoint"]!.Value<string>("id")!.ToLowerInvariant())
                .ToArray();

        return Task.CompletedTask;
    }

    public bool AccessedViaPrivateEndpoint(PrivateEndpoint privateEndpoint)
    {
        return PrivateEndpoints.Contains(privateEndpoint.Id.ToLowerInvariant());
    }
}