using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AzureDiagrams.Resources;

public class VWan : AzureResource
{
    public override string Image => "img/lib/azure2/networking/Virtual_WANs.svg";
    public override string? Fill => "#dae8fc";

    public VWan(string id, string name)
    {
        Id = id;
        Name = name;
    }

    [JsonConstructor]
    public VWan() { }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<VHub>().Where(x => x.VWanId.Equals(Id, StringComparison.InvariantCultureIgnoreCase)).ForEach(OwnsResource);
        base.BuildRelationships(allResources);
    }
   
}