using System.Collections.Generic;
using System.Linq;

namespace DrawIo.Azure.Core.Resources;

public class UserAssignedManagedIdentity : AzureResource
{
    public override string Image => "img/lib/azure2/identity/Managed_Identities.svg";

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources.OfType<IUseManagedIdentities>().Where(x => x.DoYouUseThisUserAssignedClientId(Id))
            .ForEach(mi => mi.CreateFlowBackToMe(this));
    }
}