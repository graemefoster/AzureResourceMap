using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers;

public class AppResourceRetriever : ResourceRetriever<App>
{
    public const string ConfigAppSettingsList = "config/appSettings/list";

    public AppResourceRetriever(JObject basicAzureResourceJObject) : base(basicAzureResourceJObject, "2021-01-15", true)
    {
    }

    protected override IEnumerable<(HttpMethod method, string suffix, string? version)> AdditionalResources()
    {
        yield return (HttpMethod.Post, ConfigAppSettingsList, null);
    }
}
