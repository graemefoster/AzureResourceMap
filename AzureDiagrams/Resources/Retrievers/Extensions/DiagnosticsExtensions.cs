using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources.Retrievers.Extensions;

public class DiagnosticsExtensions : IResourceExtension
{
    private string? _diagnosticsWorkspaceId;

    public (string key, HttpMethod method, string suffix, string? version)? ApiCall => ("diagnostics", HttpMethod.Get,
        "providers/microsoft.insights/diagnosticSettings", "2021-05-01-preview");

    public Task Enrich(AzureResource resource, JObject raw, Dictionary<string, JObject?> additionalResources)
    {
        var workspaces = additionalResources[ApiCall!.Value.key]!["value"]!;
        if (workspaces.Any()) _diagnosticsWorkspaceId = workspaces[0]?["properties"]?.Value<string>("workspaceId");
        return Task.CompletedTask;
    }

    public void BuildRelationships(AzureResource resource, IEnumerable<AzureResource> allResources)
    {
        if (_diagnosticsWorkspaceId != null)
        {
            var workspace = allResources.OfType<LogAnalyticsWorkspace>().SingleOrDefault(x =>
                x.Id.Equals(_diagnosticsWorkspaceId, StringComparison.InvariantCultureIgnoreCase));
            if (workspace != null) resource.CreateFlowTo(workspace, "diagnostics", Plane.Diagnostics);
        }
    }
}