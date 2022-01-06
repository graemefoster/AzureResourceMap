using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class KeyVault : AzureResource, ICanBeExposedByPrivateEndpoints
{
    public override string Image => "img/lib/azure2/security/Key_Vaults.svg";
    public string[] PrivateEndpoints { get; set; } = default!;

    public bool AccessedViaPrivateEndpoint(PrivateEndpoint privateEndpoint)
    {
        return PrivateEndpoints.Contains(privateEndpoint.Id.ToLowerInvariant());
    }

    public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
    {
        PrivateEndpoints =
            jObject["properties"]!["privateEndpointConnections"]!
                .Select(x => x["properties"]!["privateEndpoint"]!.Value<string>("id")!.ToLowerInvariant())
                .ToArray();

        return Task.CompletedTask;
    }
}