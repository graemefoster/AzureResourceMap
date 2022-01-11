using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class KeyVault : AzureResource, ICanBeAccessedViaAHostName
{
    public override string Image => "img/lib/azure2/security/Key_Vaults.svg";

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return VaultUri.Equals(hostname, StringComparison.InvariantCultureIgnoreCase);
    }

    public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
    {
        VaultUri = jObject["properties"]!.Value<string>("vaultUri")!.GetHostNameFromUrlString();
        return base.Enrich(jObject, additionalResources);
    }

    private string VaultUri { get; set; } = default!;
}