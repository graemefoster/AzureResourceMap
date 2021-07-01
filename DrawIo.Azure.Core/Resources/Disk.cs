namespace DrawIo.Azure.Core.Resources
{
    class Disk : AzureResource
    {
        public override bool IsSpecific => true;
        public string ManagedBy { get; set; }
    }
}