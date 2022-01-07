using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class StorageAccount : AzureResource
{
    public override string Image => "img/lib/azure2/storage/Storage_Accounts.svg";

    public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
    {
        return base.Enrich(jObject, additionalResources);
    }
}