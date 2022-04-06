using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources.Retrievers.Extensions;

public interface IResourceExtension
{
    /// <summary>
    /// If you need extra data from an API then declare it here
    /// </summary>
    public (string key, HttpMethod method, string suffix, string? version)? ApiCall { get; }

    /// <summary>
    /// Use the data to build the extension
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="raw"></param>
    /// <param name="additionalResources"></param>
    /// <returns></returns>
    Task Enrich(AzureResource resource, JObject raw, Dictionary<string, JObject?> additionalResources);

    /// <summary>
    /// Create any relationships between the resource holding the extension, and other resources
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="allResources"></param>
    void BuildRelationships(AzureResource resource, IEnumerable<AzureResource> allResources);
}