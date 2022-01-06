using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DrawIo.Azure.Core.Resources.Retrievers;

public class NoOpResourceRetriever : IRetrieveResource
{
    public async Task<AzureResource> FetchResource(HttpClient client)
    {
        return new IgnoreMeResource();
    }

    protected virtual IEnumerable<(HttpMethod method, string suffix)> AdditionalResources()
    {
        yield break;
    }
}