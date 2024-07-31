using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class MachineLearningWorkspace : AzureResource
{
    private string _kind;
    private string? _hubResourceId;
    private string[]? _privateEndpointLinks;
    private string? _storageAccountId;
    private string? _kvId;
    private string? _applicationInsightsId;
    private string? _containerRegistryId;
    public override string Image => "img/lib/azure2/ai_machine_learning/Machine_Learning_Studio_Workspaces.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        _kind = full.Value<string>("kind")!.ToLowerInvariant();
        
        _hubResourceId = full["properties"]!.Value<string>("hubResourceId");

        if (_kind != "project")
        {
            _storageAccountId = full["properties"]!.Value<string>("storageAccount");
            _kvId = full["properties"]!.Value<string>("keyVault");
            _applicationInsightsId = full["properties"]!.Value<string>("applicationInsights");
            _containerRegistryId = full["properties"]!.Value<string>("containerRegistry");
        }

        _privateEndpointLinks = full["properties"]!["managedNetwork"]?["outboundRules"]?
            .Children()
            .Where(x => x.First!.Value<string>("type") == "PrivateEndpoint")
            .Select(x => x.First!["destination"]!.Value<string>("serviceResourceId")!)
            .ToArray();


        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var parent = allResources.OfType<MachineLearningWorkspace>().SingleOrDefault(x => _hubResourceId == x.Id);
        if (parent != null)
        {
            parent.OwnsResource(this);
        }

        if (_storageAccountId != null)
        {
            var linkedResource = allResources.OfType<StorageAccount>()
                .SingleOrDefault(x => x.Id.ToLowerInvariant() == _storageAccountId.ToLowerInvariant());
            if (linkedResource != null)
            {
                base.CreateFlowTo(linkedResource, Plane.Runtime);
            }
        }

        if (_containerRegistryId != null)
        {
            var linkedResource = allResources.OfType<ACR>()
                .SingleOrDefault(x => x.Id.ToLowerInvariant() == _containerRegistryId.ToLowerInvariant());
            if (linkedResource != null)
            {
                base.CreateFlowTo(linkedResource, Plane.Runtime);
            }
        }

        if (_kvId != null)
        {
            var linkedResource = allResources.OfType<KeyVault>()
                .SingleOrDefault(x => x.Id.ToLowerInvariant() == _kvId.ToLowerInvariant());
            if (linkedResource != null)
            {
                base.CreateFlowTo(linkedResource, Plane.Runtime);
            }
        }

        if (_applicationInsightsId != null)
        {
            var linkedResource = allResources.OfType<AppInsights>()
                .SingleOrDefault(x => x.Id.ToLowerInvariant() == _applicationInsightsId.ToLowerInvariant());
            if (linkedResource != null)
            {
                base.CreateFlowTo(linkedResource, Plane.Diagnostics);
            }
        }

        foreach (var pe in _privateEndpointLinks ?? [])
        {
            if ((parent?._privateEndpointLinks ?? []).Contains(pe))
            {
                continue;
            }

            //We don't have the exact Private Endpoint Id - it's owned by a Microsoft network. We only see the target. So lets find a PE to that target 
            var privateEndpoint = allResources.OfType<PrivateEndpoint>().FirstOrDefault(x => x.ResourceAccessedByMe?.Id.ToLowerInvariant() == pe.ToLowerInvariant());
            if (privateEndpoint != null)
            {
                base.CreateFlowTo(privateEndpoint, Plane.Runtime);
            }
        }

        base.BuildRelationships(allResources);
    }
}