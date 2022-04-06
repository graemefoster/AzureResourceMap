using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class Bastion : AzureResource, ICanInjectIntoASubnet, ICanExposePublicIPAddresses
{
    private IpConfigurations _ipConfigurations = default!;
    public override string Image => "img/lib/azure2/networking/Connections.svg";
    public string[] PublicIpAddresses => _ipConfigurations.PublicIpAddresses;
    public string[] SubnetIdsIAmInjectedInto => _ipConfigurations.SubnetAttachments;

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        _ipConfigurations = new IpConfigurations(full);
        return base.Enrich(full, additionalResources);
    }
}