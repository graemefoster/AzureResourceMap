using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class Firewall : AzureResource, ICanInjectIntoASubnet, ICanExposePublicIPAddresses
{
    private IpConfigurations _ipConfigurations = default!;
    private IpConfigurations _mgmtIpConfigurations = default!;
    public override string Image => "img/lib/azure2/networking/Firewalls.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        _ipConfigurations = new IpConfigurations(full);
        _mgmtIpConfigurations = new IpConfigurations(full, "managementIpConfiguration");
        return base.Enrich(full, additionalResources);
    }
    public string[] PublicIpAddresses => _ipConfigurations.PublicIpAddresses.Concat(_mgmtIpConfigurations.PublicIpAddresses).ToArray();
    public string[] SubnetIdsIAmInjectedInto => _ipConfigurations.SubnetAttachments; //technically there may be another subnet associated - the mgmt one. But I can only really display one on the diagram without overcomplicating things.
}