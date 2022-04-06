using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class BigDataPool : AzureResource
{
    public override string Image => "img/lib/azure2/preview/RTOS.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        return base.Enrich(full, additionalResources);
    }
}