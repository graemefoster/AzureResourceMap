using System.Collections.Generic;
using System.Net.Http;
using AzureDiagrams.Resources.Retrievers.Extensions;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources.Retrievers.Custom;

public class AppResourceRetriever : ResourceRetriever<App>
{
    public const string ConfigAppSettingsList = "config/appSettings/list";
    public const string ConnectionStringSettingsList = "config/connectionStrings/list";

    public AppResourceRetriever(JObject basicAzureResourceJObject) : base(basicAzureResourceJObject, "2021-01-15", true,
        new IResourceExtension[] { new PrivateEndpointExtensions(), new ManagedIdentityExtension() })
    {
    }

    protected override IEnumerable<(string key, HttpMethod method, string suffix, string? version)>
        AdditionalResources()
    {
        yield return (ConfigAppSettingsList, HttpMethod.Post, ConfigAppSettingsList, null);
        yield return (ConnectionStringSettingsList, HttpMethod.Post, ConnectionStringSettingsList, null);
    }
}