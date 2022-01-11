using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class AppGateway : AzureResource, ICanBeAccessedViaAHostName, ICanInjectIntoASubnet, ICanExposePublicIPAddresses
{
    private IpConfigurations _ipConfigurations = default!;
    public override string Image => "img/lib/azure2/networking/Application_Gateways.svg";

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return _ipConfigurations.CanIAccessYouOnThisHostName(hostname);
    }

    public string[] PublicIpAddresses => _ipConfigurations.PublicIpAddresses;

    public string[] SubnetIdsIAmInjectedInto => _ipConfigurations.SubnetAttachments;

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        _ipConfigurations = new IpConfigurations(full);
        return base.Enrich(full, additionalResources);
    }
}