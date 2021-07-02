using System.Linq;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources
{
    class KeyVault : AzureResource
    {
        public override bool IsSpecific => true;
        public override bool FetchFull => true;
        public override string Image => "img/lib/azure2/security/Key_Vaults.svg";
        public override string ApiVersion => "2019-09-01";
        public string[] PrivateEndpoints { get; set; }

        public override void Enrich(JObject full)
        {
            PrivateEndpoints =
                full["properties"]["privateEndpointConnections"]
                    .Select(x => x["properties"]["privateEndpoint"].Value<string>("id").ToLowerInvariant())
                    .ToArray();        
        }
        
        public bool AccessedViaPrivateEndpoint(PrivateEndpoint privateEndpoint)
        {
            return PrivateEndpoints.Contains(privateEndpoint.Id.ToLowerInvariant());
        }
    }
}