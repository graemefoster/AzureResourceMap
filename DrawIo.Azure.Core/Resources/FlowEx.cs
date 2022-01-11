using System.Collections.Generic;
using System.Linq;

namespace DrawIo.Azure.Core.Resources;

public static class FlowEx
{
    public static void CreateFlowToHostName(
        this AzureResource connectFrom, 
        IEnumerable<AzureResource> allResources, 
        string hostName,
        string flowName)
    {
        var directNicConnections = allResources.OfType<Nic>().Where(x => x.CanIAccessYouOnThisHostName(hostName));
        if (directNicConnections.Any())
        {
            directNicConnections.ForEach(nic => connectFrom.CreateFlowTo(nic, flowName));
            return;
        }
        
        var resourcesListeningOnThisHostName = allResources.OfType<ICanBeAccessedViaAHostName>().Where(r => r.CanIAccessYouOnThisHostName(hostName)).ToArray();
        resourcesListeningOnThisHostName.ForEach(r => connectFrom.CreateFlowTo((AzureResource)r, flowName));
        
    }
}