using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Diagrams;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class Nic : AzureResource, ICanInjectIntoASubnet, ICanExposePublicIPAddresses, ICanBeAccessedViaAHostName
{
    public override string Image => "img/lib/azure2/networking/Network_Interfaces.svg";

    private IpConfigurations _ipConfigurations = default!;

    public string[] HostNames => _ipConfigurations.HostNames;

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return _ipConfigurations.CanIAccessYouOnThisHostName(hostname);
    }

    public string[] PublicIpAddresses => _ipConfigurations.PublicIpAddresses;

    public string[] SubnetIdsIAmInjectedInto => _ipConfigurations.SubnetAttachments;

    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new AzureResourceNodeBuilder(this);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<PrivateEndpoint>().Where(x => x.Nics.Contains(Id)).ForEach(vm => CreateFlowTo(vm));
        allResources.OfType<VM>().Where(x => x.Nics.Contains(Id)).ForEach(vm => CreateFlowTo(vm));
    }

    public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
    {
        _ipConfigurations = new IpConfigurations(jObject);
        return Task.CompletedTask;
    }

    public void AssignNsg(NSG nsg)
    {
        OwnsResource(nsg);
    }
}