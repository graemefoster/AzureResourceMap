using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class AppInsights : AzureResource
{
    public string Kind { get; set; } = default!;
    public override string Image => "img/lib/azure2/devops/Application_Insights.svg";
    public string ConnectionString { get; set; } = default!;

    public string InstrumentationKey { get; set; } = default!;

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        InstrumentationKey = full["properties"]!.Value<string>("InstrumentationKey")!;
        ConnectionString = full["properties"]!.Value<string>("ConnectionString")!;
        return base.Enrich(full, additionalResources);
    }
}