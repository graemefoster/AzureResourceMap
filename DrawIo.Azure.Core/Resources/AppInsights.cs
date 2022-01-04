// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Microsoft.Msagl.Core.Layout;
// using Newtonsoft.Json.Linq;
//
// namespace DrawIo.Azure.Core.Resources
// {
//     class AppInsights : AzureResource
//     {
//         public override bool IsSpecific => true;
//         public override bool FetchFull => true;
//         public string Kind { get; set; }
//         public override string Image => "img/lib/azure2/devops/Application_Insights.svg";
//         public override string ApiVersion => "2020-02-02";
//
//         public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
//         {
//             InstrumentationKey = full["properties"]!.Value<string>("InstrumentationKey")!;
//             ConnectionString = full["properties"]!.Value<string>("ConnectionString")!;
//             return base.Enrich(full, additionalResources);
//         }
//
//         public string ConnectionString { get; set; }
//
//         public string InstrumentationKey { get; set; }
//     }
// }