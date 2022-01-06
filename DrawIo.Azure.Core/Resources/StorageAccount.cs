using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class StorageAccount : AzureResource, ICanBeExposedByPrivateEndpoints
{
    public override string Image => "img/lib/azure2/storage/Storage_Accounts.svg";

    private string[] PrivateEndpoints { get; set; } = default!;

    /// <summary>
    ///     If the storage account is exposed by a private endpoint, it will be in this list.
    /// </summary>
    public List<PrivateEndpoint> ExposedByPrivateEndpoints { get; set; } = new();

    public bool AccessedViaPrivateEndpoint(PrivateEndpoint privateEndpoint)
    {
        var exposedByThisPrivateEndpoint = PrivateEndpoints.Contains(privateEndpoint.Id.ToLowerInvariant());
        if (exposedByThisPrivateEndpoint) ExposedByPrivateEndpoints.Add(privateEndpoint);

        return exposedByThisPrivateEndpoint;
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