using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class MachineLearningWorkspace : AzureResource
{
    const  string AIStudioImage = "data:image/svg+xml,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxOCAxOCIgaGVpZ2h0PSIxOCIgd2lkdGg9IjE4IiBpZD0idXVpZC02YjgzODBjMy0wZWU1LTRjNDQtOTJhMi1mMTg1YzgyZGI2YmEiPjxkZWZzPjxsaW5lYXJHcmFkaWVudCBncmFkaWVudFVuaXRzPSJ1c2VyU3BhY2VPblVzZSIgZ3JhZGllbnRUcmFuc2Zvcm09InRyYW5zbGF0ZSg2MTcuMTI2IC0yMDUuNzU4KSBzY2FsZSgxIC0xKSIgeTI9Ii0yMDYuMjIiIHgyPSItNjA2LjYiIHkxPSItMjE4LjM3OCIgeDE9Ii02MDMuNTYzIiBpZD0idXVpZC0wNTg3NmM3Mi04ZjI2LTQwZGEtOTk2ZS1hNDg4MTcyZWMwNzIiPjxzdG9wIHN0b3AtY29sb3I9IiM3MTI1NzUiIG9mZnNldD0iMCIvPjxzdG9wIHN0b3AtY29sb3I9IiM5YTI4ODQiIG9mZnNldD0iLjA5Ii8+PHN0b3Agc3RvcC1jb2xvcj0iI2JmMmM5MiIgb2Zmc2V0PSIuMTgiLz48c3RvcCBzdG9wLWNvbG9yPSIjZGEyZTljIiBvZmZzZXQ9Ii4yNyIvPjxzdG9wIHN0b3AtY29sb3I9IiNlYjMwYTIiIG9mZnNldD0iLjM0Ii8+PHN0b3Agc3RvcC1jb2xvcj0iI2YxMzFhNSIgb2Zmc2V0PSIuNCIvPjxzdG9wIHN0b3AtY29sb3I9IiNlYzMwYTMiIG9mZnNldD0iLjUiLz48c3RvcCBzdG9wLWNvbG9yPSIjZGYyZjllIiBvZmZzZXQ9Ii42MSIvPjxzdG9wIHN0b3AtY29sb3I9IiNjOTJkOTYiIG9mZnNldD0iLjcyIi8+PHN0b3Agc3RvcC1jb2xvcj0iI2FhMmE4YSIgb2Zmc2V0PSIuODMiLz48c3RvcCBzdG9wLWNvbG9yPSIjODMyNjdjIiBvZmZzZXQ9Ii45NSIvPjxzdG9wIHN0b3AtY29sb3I9IiM3MTI1NzUiIG9mZnNldD0iMSIvPjwvbGluZWFyR3JhZGllbnQ+PGxpbmVhckdyYWRpZW50IGdyYWRpZW50VW5pdHM9InVzZXJTcGFjZU9uVXNlIiBncmFkaWVudFRyYW5zZm9ybT0idHJhbnNsYXRlKDYxNy4xMjYgLTIwNS43NTgpIHNjYWxlKDEgLTEpIiB5Mj0iLTIyMy4xNzUiIHgyPSItNjAyLjQxMiIgeTE9Ii0yMDYuMDI1IiB4MT0iLTYwMi40MTIiIGlkPSJ1dWlkLWM0YTJmNjI3LWQ3MzAtNDQ3ZS05MTUyLTYyMDA5YzY0YzM2MSI+PHN0b3Agc3RvcC1jb2xvcj0iI2RhN2VkMCIgb2Zmc2V0PSIwIi8+PHN0b3Agc3RvcC1jb2xvcj0iI2IxN2JkNSIgb2Zmc2V0PSIuMDgiLz48c3RvcCBzdG9wLWNvbG9yPSIjODc3OGRiIiBvZmZzZXQ9Ii4xOSIvPjxzdG9wIHN0b3AtY29sb3I9IiM2Mjc2ZTEiIG9mZnNldD0iLjMiLz48c3RvcCBzdG9wLWNvbG9yPSIjNDU3NGU1IiBvZmZzZXQ9Ii40MSIvPjxzdG9wIHN0b3AtY29sb3I9IiMyZTcyZTgiIG9mZnNldD0iLjU0Ii8+PHN0b3Agc3RvcC1jb2xvcj0iIzFkNzFlYiIgb2Zmc2V0PSIuNjciLz48c3RvcCBzdG9wLWNvbG9yPSIjMTQ3MWVjIiBvZmZzZXQ9Ii44MSIvPjxzdG9wIHN0b3AtY29sb3I9IiMxMTcxZWQiIG9mZnNldD0iMSIvPjwvbGluZWFyR3JhZGllbnQ+PGxpbmVhckdyYWRpZW50IGdyYWRpZW50VW5pdHM9InVzZXJTcGFjZU9uVXNlIiBncmFkaWVudFRyYW5zZm9ybT0idHJhbnNsYXRlKDYxNy4xMjYgLTIwNS43NTgpIHNjYWxlKDEgLTEpIiB5Mj0iLTIyNC42NDQiIHgyPSItNjE0LjgwNyIgeTE9Ii0yMDYuNDE0IiB4MT0iLTYwMy40MzgiIGlkPSJ1dWlkLTVhNGNmMjE1LTQ5MzItNGYxMi04YWYxLTFiNjgzM2RmMjU5YyI+PHN0b3Agc3RvcC1jb2xvcj0iI2RhN2VkMCIgb2Zmc2V0PSIwIi8+PHN0b3Agc3RvcC1jb2xvcj0iI2I3N2JkNCIgb2Zmc2V0PSIuMDUiLz48c3RvcCBzdG9wLWNvbG9yPSIjOTA3OWRhIiBvZmZzZXQ9Ii4xMSIvPjxzdG9wIHN0b3AtY29sb3I9IiM2ZTc3ZGYiIG9mZnNldD0iLjE4Ii8+PHN0b3Agc3RvcC1jb2xvcj0iIzUxNzVlMyIgb2Zmc2V0PSIuMjUiLz48c3RvcCBzdG9wLWNvbG9yPSIjMzk3M2U3IiBvZmZzZXQ9Ii4zMyIvPjxzdG9wIHN0b3AtY29sb3I9IiMyNzcyZTkiIG9mZnNldD0iLjQyIi8+PHN0b3Agc3RvcC1jb2xvcj0iIzFhNzFlYiIgb2Zmc2V0PSIuNTQiLz48c3RvcCBzdG9wLWNvbG9yPSIjMTM3MWVjIiBvZmZzZXQ9Ii42OCIvPjxzdG9wIHN0b3AtY29sb3I9IiMxMTcxZWQiIG9mZnNldD0iMSIvPjwvbGluZWFyR3JhZGllbnQ+PC9kZWZzPjxwYXRoIHN0cm9rZS13aWR0aD0iMCIgZmlsbC1ydWxlPSJldmVub2RkIiBmaWxsPSJ1cmwoI3V1aWQtMDU4NzZjNzItOGYyNi00MGRhLTk5NmUtYTQ4ODE3MmVjMDcyKSIgZD0ibTEyLjA2MS4wMTJjLjUzNCwwLDEuMDA4LjQwMSwxLjE3OC45ODRzMS4xNjYsNC4xOSwxLjE2Niw0LjE5djcuMTY2aC0zLjYwN2wuMDczLTEyLjM1MmgxLjE5di4wMTJaIi8+PHBhdGggc3Ryb2tlLXdpZHRoPSIwIiBmaWxsPSJ1cmwoI3V1aWQtYzRhMmY2MjctZDczMC00NDdlLTkxNTItNjIwMDljNjRjMzYxKSIgZD0ibTE3LjM1Niw1LjYxMWMwLS4yNTUtLjIwNi0uNDQ5LS40NDktLjQ0OWgtMi4xMjZjLTEuNDk0LDAtMi43MDksMS4yMTUtMi43MDksMi43MDl2NC40OTRoMi41NzVjMS40OTQsMCwyLjcwOS0xLjIxNSwyLjcwOS0yLjcwOXYtNC4wNDVaIi8+PHBhdGggc3Ryb2tlLXdpZHRoPSIwIiBmaWxsLXJ1bGU9ImV2ZW5vZGQiIGZpbGw9InVybCgjdXVpZC01YTRjZjIxNS00OTMyLTRmMTItOGFmMS0xYjY4MzNkZjI1OWMpIiBkPSJtMTIuMDYxLjAxMmMtLjQxMywwLS43NDEuMzI4LS43NDEuNzQxbC0uMDczLDEzLjY0YzAsMS45OTItMS42MTUsMy42MDctMy42MDcsMy42MDdIMS4wOTNjLS4zMTYsMC0uNTIyLS4zMDQtLjQyNS0uNTk1TDUuOTE1LDIuNDI5QzYuNDI1Ljk4NCw3Ljc4NS4wMTIsOS4zMTYuMDEyaDIuNzU3LS4wMTJaIi8+PC9zdmc+";
    
    private string _kind = default!;
    private string? _hubResourceId;
    private string[]? _privateEndpointLinks;
    private string? _storageAccountId;
    private string? _kvId;
    private string? _applicationInsightsId;
    private string? _containerRegistryId;
    private VNet? _managedVnet;

    public override string Image => _kind switch
    {
        "hub" => AIStudioImage,
        _ => "img/lib/azure2/ai_machine_learning/Machine_Learning_Studio_Workspaces.svg"
    };

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

            if (full["properties"]!["managedNetwork"] != null)
            {
                var subnet = new VNet.Subnet(Name + "-subnet", "");
                _managedVnet = new VNet(Guid.NewGuid().ToString(), Name + "-vnet", new[] { subnet });
            }
        }

        _privateEndpointLinks = full["properties"]!["managedNetwork"]?["outboundRules"]?
            .Children()
            .Where(x => x.First!.Value<string>("type") == "PrivateEndpoint")
            .Select(x => x.First!["destination"]!.Value<string>("serviceResourceId")!)
            .ToArray() ?? [];

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
        
        _privateEndpointLinks!.ForEach(x =>
        {
            if (parent?._privateEndpointLinks!.Contains(x) ?? false) return;
            
            var resource = allResources.SingleOrDefault(r => r.Id.ToLowerInvariant() == x.ToLowerInvariant());
            if (resource != null)
            {
                CreateFlowTo(resource, "Private Link", Plane.Runtime);
            }
        });
        
        base.BuildRelationships(allResources);
    }
}