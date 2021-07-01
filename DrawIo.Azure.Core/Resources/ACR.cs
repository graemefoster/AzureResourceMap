namespace DrawIo.Azure.Core.Resources
{
    class ACR : AzureResource
    {
        public override bool IsSpecific => true;
        public Identity? Identity { get; set; }
    }
}