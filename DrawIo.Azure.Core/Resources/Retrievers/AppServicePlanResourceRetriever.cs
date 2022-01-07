using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers;

public class AppServicePlanResourceRetriever : ResourceRetriever<ASP>
{
    public const string DiagnosticSettings = "providers/microsoft.insights/diagnosticSettings";

    public AppServicePlanResourceRetriever(JObject basicAzureResourceJObject) : base(basicAzureResourceJObject, "2021-03-01", true)
    {
    }

    protected override IEnumerable<(HttpMethod method, string suffix, string? version)> AdditionalResources()
    {
        yield return (HttpMethod.Get, DiagnosticSettings, "2021-05-01-preview");
    }
}