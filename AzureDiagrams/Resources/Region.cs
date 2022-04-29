using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureDiagrams.Resources;

public class Region : AzureResource
{
    public override bool IsPureContainer => true;
    public override string Fill => "#DAE8FC";

    public Region(string? location)
    {
        Location = location ?? "global";
        Name = Location;
        Id = $"azdatacentre-{location}";
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.Except(new [] {this}).Where(x => !x.ContainedByAnotherResource).Where(x => (x.Location ?? "global").Equals(Location, StringComparison.InvariantCultureIgnoreCase)).ForEach(OwnsResource);
        base.BuildRelationships(allResources);
    }
}