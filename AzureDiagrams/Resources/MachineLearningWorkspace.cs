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
    public override string Image => "img/lib/azure2/ai_machine_learning/Machine_Learning_Studio_Workspaces.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        _kind = full.Value<string>("kind")!.ToLowerInvariant();
        _hubResourceId = full["properties"]!.Value<string>("hubResourceId");
        _privateEndpointLinks = full["properties"]!["managedNetwork"]?["outboundRules"]?
            .Children()
            .Where(x => x.First!.Value<string>("type") == "PrivateEndpoint")
            .Select(x => x.First!["destination"]!.Value<string>("serviceResourceId")!)
            .ToArray();


        return base.Enrich(full, additionalResources);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        if (_kind == "project")
        {
            var parent = allResources.OfType<MachineLearningWorkspace>().SingleOrDefault(x => _hubResourceId == x.Id);
            if (parent != null)
            {
                parent.OwnsResource(this);
            }
        }

        foreach (var pe in _privateEndpointLinks ?? [])
        {
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