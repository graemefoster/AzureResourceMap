using System.Net.Http;
using System.Threading.Tasks;

namespace AzureDiagrams.Resources.Retrievers;

public interface IRetrieveResource
{
    Task<AzureResource> FetchResource(HttpClient client);
}