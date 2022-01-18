using System.Collections.Generic;
using System.Linq;

namespace DrawIo.Azure.Core.Resources;

public class CommonDiagnostics : AzureResource
{
    public override bool IsPureContainer => true;

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<LogAnalyticsWorkspace>().ForEach(OwnsResource);
        allResources.OfType<AppInsights>().ForEach(OwnsResource);
        base.BuildRelationships(allResources);
    }
}