using System.Net.Http;
using System.Threading.Tasks;

namespace DrawIo.Azure.Core.Resources.Retrievers;

public interface IRetrieveResource
{
    Task<AzureResource> FetchResource(HttpClient client);
}