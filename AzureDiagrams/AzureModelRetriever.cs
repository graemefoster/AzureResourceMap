using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using AzureDiagrams.Resources;
using AzureDiagrams.Resources.Retrievers;

namespace AzureDiagrams;

public class AzureModelRetriever
{
    public async Task<AzureResource[]> Retrieve(TokenCredential tokenCredential, CancellationToken cancellationToken,
        Guid subscriptionId, string? tenantId = null, params string[] resourceGroups)
    {
        var token = await tokenCredential.GetTokenAsync(
            new TokenRequestContext(new[] { "https://management.azure.com/" }),
            cancellationToken);

        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://management.azure.com/");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        var armClient = new ArmClient(httpClient, tokenCredential);

        var expandedResourceGroups = new List<string>();
        await foreach (var rg in armClient.FindResourceGroups(subscriptionId, resourceGroups)
                           .WithCancellation(cancellationToken))
        {
            expandedResourceGroups.Add(rg);
        }

        var resources = new List<AzureResource>();
        await foreach (var resource in armClient.Retrieve(subscriptionId, expandedResourceGroups)
                           .WithCancellation(cancellationToken)) resources.Add(resource);
        var additionalNodes = resources.SelectMany(x => x.DiscoverNewNodes(resources));

        //create some common nodes to represent common platform groupings (AAD, Diagnostics)
        var aad = new AzureActiveDirectory { Id = CommonResources.AAD, Name = "Azure Active Directory" };
        var distinctRegions = resources.Select(x => x.Location).Distinct(StringComparer.InvariantCultureIgnoreCase)
            .ToArray();
        var regions = distinctRegions.Select(x => new Region(x)).ToArray();
        var core = distinctRegions.Select(x => new CoreServices
            { Id = $"{CommonResources.CoreServices}-{x}", Name = x, Location = x }).ToArray();
        var pips = distinctRegions.Select(x => new PublicIpAddresses
            { Id = $"{CommonResources.PublicIpAddresses}-{x}", Name = x, Location = x }).ToArray();
        var diagnostics = distinctRegions.Select(x => new CommonDiagnostics
            { Id = $"{CommonResources.Diagnostics}-{x}", Name = x, Location = x }).ToArray();
        var allNodes = resources.Concat(additionalNodes).Concat(
                new AzureResource[] { aad })
            .Concat(core)
            .Concat(diagnostics)
            .Concat(pips)
            .Concat(regions) //needs to come last to make sure we don't assign owenership of things multiple times.
            .ToArray();

        //Discover hidden links that aren't obvious through the resource manager
        //For example, a NIC / private endpoint linked to a subnet
        foreach (var resource in allNodes) resource.BuildRelationships(allNodes);

        return allNodes;
    }
}