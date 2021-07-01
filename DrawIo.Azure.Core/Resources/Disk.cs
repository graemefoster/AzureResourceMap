namespace DrawIo.Azure.Core.Resources
{
    class Disk : AzureResource
    {
        public override bool IsSpecific => true;
        public string ManagedBy { get; set; }
        public override string Image => "img/lib/azure2/compute/Disks.svg";
    }
}