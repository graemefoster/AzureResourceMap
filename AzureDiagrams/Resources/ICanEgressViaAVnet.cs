using System.Collections.Generic;

namespace DrawIo.Azure.Core.Resources;

/// <summary>
/// If your traffic may flow via a different resource (example might be VNet integration) then implement this.
/// Classes like HostNameDiscoverer will use it when building relationships.
/// </summary>
interface ICanEgressViaAVnet
{
    AzureResource EgressResource();
}