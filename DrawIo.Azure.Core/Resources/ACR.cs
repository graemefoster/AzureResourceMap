namespace DrawIo.Azure.Core.Resources;

internal class ACR : AzureResource
{
    public Identity? Identity { get; set; }
    public override string Image => "img/lib/azure2/containers/Container_Registries.svg";
}