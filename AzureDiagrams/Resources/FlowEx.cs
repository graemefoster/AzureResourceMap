using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureDiagrams.Resources;

public static class FlowEx
{
    public static void CreateFlowToHostName(
        this AzureResource fromResource,
        IEnumerable<AzureResource> allResources,
        string hostName,
        string flowName,
        Plane plane)
    {
        var possibleHosts = allResources.OfType<ICanBeAccessedViaAHostName>()
            .Where(x => x.CanIAccessYouOnThisHostName(hostName));
        var host = possibleHosts.SingleOrDefault(x => !(x is Nic));
        if (host != null)
        {
            fromResource.CreateLayer7Flow(
                allResources,
                (AzureResource)host,
                flowName,
                hn => hn.Contains(hostName, StringComparer.InvariantCultureIgnoreCase),
                plane);
        }
    }

    public static void CreateLayer7Flow(
        this AzureResource fromResource,
        IEnumerable<AzureResource> allResources,
        AzureResource connectTo,
        string flowName,
        Func<string[], bool> nicHostNameCheck,
        Plane plane)
    {
        var nics = allResources.OfType<Nic>().Where(nic => nicHostNameCheck(nic.HostNames)).ToArray();

        if (fromResource is ICanEgressViaAVnet vnetEgress)
        {
            var egress = vnetEgress.EgressResource();
            if (egress != fromResource)
            {
                fromResource.CreateFlowTo(egress, plane);
            }

            if (nics.Any())
            {
                nics.ForEach(nic => egress.CreateFlowTo(nic, flowName, plane));
            }
            else
            {
                //Assume all traffic going via vnet for simplicity. We can get clever if we want later around public / private IP addresses / introspecting routes, etc.
                egress.CreateFlowTo(connectTo, flowName, plane);
            }
        }
        else
        {
            //direct flow to the resource (no vnet integration).
            //If we found a nic that listened on the hostname then flow to that.
            if (nics.Any())
            {
                nics.ForEach(nic => fromResource.CreateFlowTo(nic, flowName, plane));
            }
            else
            {
                fromResource.CreateFlowTo(connectTo, flowName, plane);
            }
        }
    }
}