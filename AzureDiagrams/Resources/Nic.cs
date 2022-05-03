using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

[DebuggerDisplay("{Type}/{Name}")]
public class Nic : AzureResource, ICanInjectIntoASubnet, ICanExposePublicIPAddresses, ICanBeAccessedViaAHostName
{
    public override string Image => "img/lib/azure2/networking/Network_Interfaces.svg";

    private IpConfigurations _ipConfigurations = default!;
    public PrivateEndpoint? ConnectedPrivateEndpoint { get; private set; }

    public string[] HostNames => _ipConfigurations.HostNames;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return _ipConfigurations.CanIAccessYouOnThisHostName(hostname);
    }

    public string[] PublicIpAddresses => _ipConfigurations.PublicIpAddresses;

    public string[] SubnetIdsIAmInjectedInto => _ipConfigurations.SubnetAttachments.Distinct().ToArray();

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        ConnectedPrivateEndpoint = allResources.OfType<PrivateEndpoint>().SingleOrDefault(x => x.Nics.Contains(Id));
        ConnectedPrivateEndpoint?.CreateFlowTo(this, Plane.All);
        allResources.OfType<VM>().Where(x => x.Nics.Contains(Id)).ForEach(vm => CreateFlowTo(vm, Plane.All));
        base.BuildRelationships(allResources);
    }

    public override Task Enrich(JObject jObject, Dictionary<string, JObject?> additionalResources)
    {
        _ipConfigurations = new IpConfigurations(jObject);
        return Task.CompletedTask;
    }

    public void AssignNsg(NSG nsg)
    {
        OwnsResource(nsg);
    }
}