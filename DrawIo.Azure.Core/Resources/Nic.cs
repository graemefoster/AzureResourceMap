using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class Nic : AzureResource
{
    public override bool FetchFull => true;
    public override string Image => "img/lib/azure2/networking/Network_Interfaces.svg";
    public string[] PublicIpAddresses { get; set; }

    private IEnumerable<string> _networkAttachments { get; set; }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<PrivateEndpoint>().Where(x => x.Nics.Contains(Id)).ForEach(CreateFlowTo);
        allResources.OfType<VM>().Where(x => x.Nics.Contains(Id)).ForEach(CreateFlowTo);

        var subnets = _networkAttachments
            .Select(x =>
            {
                var segments = x.Split('/');
                return new { vnet = segments.ElementAt(segments.Length - 3), subnet = segments.Last() };
            })
            .ToArray();

        foreach (var subnet in subnets)
        {
            var vNet = allResources.OfType<VNet>().Single(x => x.Name == subnet.vnet);
            vNet.InjectResourceInto(this, subnet.subnet);
            foreach (var associatedWithNic in
                     allResources.OfType<IAssociateWithNic>().Where(x =>
                         x.Nics.Any(nic => nic.Equals(Id, StringComparison.InvariantCultureIgnoreCase))))
                vNet.InjectResourceInto((AzureResource)associatedWithNic, subnet.subnet);
        }
    }

    public override Task Enrich(JObject jObject, Dictionary<string, JObject> additionalResources)
    {
        PublicIpAddresses = jObject["properties"]!["ipConfigurations"]!
            .Select(x =>
                x["properties"]!["publicIPAddress"] != null
                    ? x["properties"]!["publicIPAddress"]!.Value<string>("id")!.ToLowerInvariant()
                    : null)
            .Where(x => x != null)
            .ToArray()!;

        _networkAttachments = jObject["properties"]!["ipConfigurations"]!
            .Select(x =>
                x["properties"]!["subnet"] != null
                    ? x["properties"]!["subnet"]!.Value<string>("id")!.ToLowerInvariant()
                    : null)
            .Where(x => x != null)
            .Select(x => x!);

        return Task.CompletedTask;
    }

    public bool ExposedBy(PIP pip)
    {
        return PublicIpAddresses.Contains(pip.Id.ToLowerInvariant());
    }
}