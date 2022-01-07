using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class KeyVault : AzureResource, ICanBeAccessedViaHttp
{
    public override string Image => "img/lib/azure2/security/Key_Vaults.svg";

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        throw new NotImplementedException();
    }

    public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
    {
        return base.Enrich(jObject, additionalResources);
    }
}