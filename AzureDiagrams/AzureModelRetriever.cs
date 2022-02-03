using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using DrawIo.Azure.Core.Resources;
using DrawIo.Azure.Core.Resources.Retrievers;

namespace DrawIo.Azure.Core;

public class AzureModelRetriever
{
    public async Task<AzureResource[]> Retrieve(TokenCredential tokenCredential, CancellationToken cancellationToken, Guid subscriptionId, params string[] resourceGroups)
    {
        var token = await tokenCredential.GetTokenAsync(
            new TokenRequestContext(new[] { "https://management.azure.com/" }),
            cancellationToken);

        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://management.azure.com/");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        var armClient = new ArmClient(httpClient, tokenCredential);
        var resources = (await armClient.Retrieve(subscriptionId, resourceGroups)).ToArray();
        var additionalNodes = resources.SelectMany(x => x.DiscoverNewNodes());

        //create some common nodes to represent common platform groupings (AAD, Diagnostics)
        var aad = new AzureActiveDirectory { Id = CommonResources.AAD, Name = "Azure Active Directory" };
        var core = new CoreServices { Id = CommonResources.CoreServices, Name = "Core Services" };
        var diagnostics = new CommonDiagnostics { Id = CommonResources.Diagnostics, Name = "Diagnostics" };
        var allNodes = resources.Concat(additionalNodes).Concat(new AzureResource[] { aad, diagnostics, core })
            .ToArray();

        //Discover hidden links that aren't obvious through the resource manager
        //For example, a NIC / private endpoint linked to a subnet
        foreach (var resource in allNodes) resource.BuildRelationships(allNodes);

        return allNodes;
    }
}