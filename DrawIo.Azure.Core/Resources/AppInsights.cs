using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class AppInsights : AzureResource
{
    public override bool FetchFull => true;
    public string Kind { get; set; }
    public override string Image => "img/lib/azure2/devops/Application_Insights.svg";
    public override string ApiVersion => "2020-02-02";

    public string ConnectionString { get; set; }

    public string InstrumentationKey { get; set; }

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        InstrumentationKey = full["properties"]!.Value<string>("InstrumentationKey")!;
        ConnectionString = full["properties"]!.Value<string>("ConnectionString")!;
        return base.Enrich(full, additionalResources);
    }
}