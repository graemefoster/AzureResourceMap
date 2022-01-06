namespace DrawIo.Azure.Core.Resources;

internal class AKS : AzureResource
{
    public Identity? Identity { get; set; }
    public override string Image => "img/lib/azure2/containers/Kubernetes_Services.svg";
}