using DrawIo.Azure.Core.Resources.Retrievers.Extensions;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources.Retrievers.Custom;

/// <summary>
/// Had hoped to fetch linked-services but you query them on a different api, need a different token, and that api might be on a private endpoint... So leaving for now :(
/// </summary>
public class SynapseRetriever : ResourceRetriever<Synapse>
{

    public SynapseRetriever(JObject basicAzureResourceJObject) : base(basicAzureResourceJObject,
        fetchFullResource: true, apiVersion: "2021-06-01",
        extensions: new IResourceExtension[]
            { new DiagnosticsExtensions(), new PrivateEndpointExtensions(), new ManagedIdentityExtension() })
    {
    }
}