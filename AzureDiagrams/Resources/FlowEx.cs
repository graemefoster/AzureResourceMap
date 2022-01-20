using System;
using System.Collections.Generic;
using System.Linq;

namespace DrawIo.Azure.Core.Resources;

public static class FlowEx
{
    // public static void CreateFlowToHostName(
    //     this AzureResource connectFrom, 
    //     IEnumerable<AzureResource> allResources, 
    //     string hostName,
    //     string flowName)
    // {
    //     var directNicConnections = allResources.OfType<Nic>().Where(x => x.CanIAccessYouOnThisHostName(hostName));
    //     if (directNicConnections.Any())
    //     {
    //         directNicConnections.ForEach(nic => connectFrom.CreateFlowTo(nic, flowName));
    //         return;
    //     }
    //     
    //     var resourcesListeningOnThisHostName = allResources.OfType<ICanBeAccessedViaAHostName>().Where(r => r.CanIAccessYouOnThisHostName(hostName)).ToArray();
    //     resourcesListeningOnThisHostName.ForEach(r => connectFrom.CreateFlowTo((AzureResource)r, flowName));
    //     
    // }
    //

    public static void CreateFlowToHostName(
        this AzureResource fromResource,
        IEnumerable<AzureResource> allResources,
        string hostName,
        string flowName)
    {
        var possibleHosts = allResources.OfType<ICanBeAccessedViaAHostName>().Where(x => x.CanIAccessYouOnThisHostName(hostName));
        var host = possibleHosts.SingleOrDefault(x => !(x is Nic));
        if (host != null)
        {
            fromResource.CreateLayer7Flow(
                allResources,
                (AzureResource)host,
                flowName,
                hn => hn.Contains(hostName, StringComparer.InvariantCultureIgnoreCase));
        }
    }

    public static void CreateLayer7Flow(
        this AzureResource fromResource,
        IEnumerable<AzureResource> allResources,
        AzureResource connectTo,
        string flowName,
        Func<string[], bool> nicHostNameCheck)
    {
        var nics = allResources.OfType<Nic>().Where(nic => nicHostNameCheck(nic.HostNames)).ToArray();

        if (fromResource is ICanEgressViaAVnet vnetEgress)
        {
            var egress = vnetEgress.EgressResource();
            if (egress != fromResource)
            {
                fromResource.CreateFlowTo(egress);
            }

            if (nics.Any())
            {
                nics.ForEach(nic => egress.CreateFlowTo(nic, flowName));
            }
            else
            {
                //Assume all traffic going via vnet for simplicity. We can get clever if we want later around public / private IP addresses / introspecting routes, etc.
                egress.CreateFlowTo(connectTo, flowName);
            }
        }
        else
        {
            //direct flow to the resource (no vnet integration)
            fromResource.CreateFlowTo(connectTo, flowName);
        }
    }
}