using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources.Retrievers.Extensions;

public class ManagedIdentityExtension : IResourceExtension
{
    private Identity? Identity { get; set; }

    public (string key, HttpMethod method, string suffix, string? version)? ApiCall { get; }

    public Task Enrich(AzureResource resource, JObject raw, Dictionary<string, JObject?> additionalResources)
    {
        Identity = raw["identity"]?.ToObject<Identity>();
        return Task.CompletedTask;
    }

    public void BuildRelationships(AzureResource resource, IEnumerable<AzureResource> allResources)
    {
        Identity?.UserAssignedIdentities?.Keys.ForEach(i =>
            allResources.OfType<UserAssignedManagedIdentity>().Where(uami => uami.Id.Equals(i, StringComparison.InvariantCultureIgnoreCase))
                .ForEach(uami => resource.CreateFlowTo(uami, "AAD Identity", Plane.Identity)));
    }
}