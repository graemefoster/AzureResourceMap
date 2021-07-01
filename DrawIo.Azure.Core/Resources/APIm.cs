namespace DrawIo.Azure.Core.Resources
{
    class APIm : AzureResource
    {
        public override bool IsSpecific => true;
        public Identity? Identity { get; set; }
    }
}