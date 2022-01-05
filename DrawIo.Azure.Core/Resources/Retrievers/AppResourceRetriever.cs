using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers;

public class AppResourceRetriever : ResourceRetriever<App>
{
    private const string ConfigAppSettingsList = "config/appSettings/list";

    public AppResourceRetriever(BasicAzureResourceInfo basicAzureResourceInfo) : base(basicAzureResourceInfo)
    {
    }

    public override bool FetchFull => true;

    public override string ApiVersion => "2021-01-15";

    protected override IEnumerable<(HttpMethod method, string suffix)> AdditionalResources()
    {
        yield return (HttpMethod.Post, ConfigAppSettingsList);
    }

    protected override AzureResource BuildResource(BasicAzureResourceInfo basicAzureResourceInfo, JObject resource, Dictionary<string, JObject> additionalResources)
    {
        var properties = resource["properties"]!.ToObject<App.AppProperties>()!;

        properties.PrivateEndpoints =
            resource["properties"]!["privateEndpointConnections"]!
                .Select(x => x["properties"]!["privateEndpoint"].Value<string>("id").ToLowerInvariant())
                .ToArray();


        var config = additionalResources[ConfigAppSettingsList];
        var appSettings = config["properties"]!.ToObject<Dictionary<string, object>>()!;

        if (appSettings.ContainsKey("APPINSIGHTS_INSTRUMENTATIONKEY"))
        {
            var appInsightsKey = (string)appSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];
        }

        var connectedStorageAccounts = appSettings
            .Values
            .OfType<string>()
            .Where(appSetting => appSetting.Contains("DefaultEndpointsProtocol") &&
                                 appSetting.Contains("AccountName") &&
                                 appSetting.Contains("EndpointSuffix"))
            .Select(x =>
            {
                var parts = x!.Split(';')
                    .Select(x =>
                        new KeyValuePair<string, string>(x.Split('=')[0].ToLowerInvariant(),
                            x.Split('=')[1].ToLowerInvariant()))
                    .ToDictionary(x => x.Key, x => x.Value);

                return (parts["accountname"], "." + parts["endpointsuffix"]);
            })
            .ToArray();

        return new App()
        {
            Id = basicAzureResourceInfo.Id,
            Identity = 
        }
        return base.BuildResource(resource, additionalResources);
    }
}