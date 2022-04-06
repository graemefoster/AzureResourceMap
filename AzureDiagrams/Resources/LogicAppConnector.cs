using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class LogicAppConnector : AzureResource
{
    public override string Image => "img/lib/azure2/general/Input_Output.svg";
    
    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        //TODO - is there a way to get info about the connection (api host name, or something like that?)
        return base.Enrich(full, additionalResources);
    }
}