namespace DrawIo.Azure.Core.Resources;

internal class Disk : AzureResource
{
    public string ManagedBy { get; set; } = default!;
    public override string Image => "img/lib/azure2/compute/Disks.svg";
}