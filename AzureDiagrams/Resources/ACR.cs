using System;

namespace DrawIo.Azure.Core.Resources;

internal class ACR : AzureResource, ICanBeAccessedViaAHostName
{
    public override string Image => "img/lib/azure2/containers/Container_Registries.svg";

    public bool CanIAccessYouOnThisHostName(string hostname)
    {
        return hostname.Equals($"{Name}.azurecr.io", StringComparison.InvariantCultureIgnoreCase);
    }
}