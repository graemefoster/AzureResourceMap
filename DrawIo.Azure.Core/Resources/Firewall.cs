using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class Firewall : AzureResource, ICanInjectIntoASubnet, ICanExposePublicIPAddresses
{
    private IpConfigurations _ipConfigurations = default!;
    public override string Image => "img/lib/azure2/networking/Firewalls.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        _ipConfigurations = new IpConfigurations(full);
        return base.Enrich(full, additionalResources);
    }
    public string[] PublicIpAddresses => _ipConfigurations.PublicIpAddresses;
    public string[] SubnetIdsIAmInjectedInto => _ipConfigurations.SubnetAttachments;
}