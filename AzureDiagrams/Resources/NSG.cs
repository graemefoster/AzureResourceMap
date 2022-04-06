using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AzureDiagrams.Resources;

public class NSG : AzureResource
{
    private string[] _networkInterfacesBoundTo = default!;
    private string[] _subnetsBoundTo = default!;

    public override string Image => "img/lib/azure2/networking/Network_Security_Groups.svg";

    public override Task Enrich(JObject full, Dictionary<string, JObject?> additionalResources)
    {
        if (full["properties"]!["networkInterfaces"] != null)
            _networkInterfacesBoundTo =
                full["properties"]!["networkInterfaces"]!.Select(x => x.Value<string>("id")!).ToArray();
        else
            _networkInterfacesBoundTo = Array.Empty<string>();

        if (full["properties"]!["subnets"] != null)
            _subnetsBoundTo =
                full["properties"]!["subnets"]!.Select(x => x.Value<string>("id")!).ToArray();
        else
            _subnetsBoundTo = Array.Empty<string>();

        return Task.CompletedTask;
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        _subnetsBoundTo.ForEach(x =>
            allResources.OfType<VNet>().Single(vNet => vNet.Id == string.Join('/', x.Split('/')[..^2]))
                .AssignNsg(this, x.Split('/')[^1]));
        _networkInterfacesBoundTo.ForEach(x => allResources.OfType<Nic>().Single(nic => nic.Id == x).AssignNsg(this));
        base.BuildRelationships(allResources);
    }
}