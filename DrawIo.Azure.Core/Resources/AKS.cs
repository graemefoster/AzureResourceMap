namespace DrawIo.Azure.Core.Resources
{
    class AKS : AzureResource
    {
        public override bool IsSpecific => true;
        public Identity? Identity { get; set; }
    }
}