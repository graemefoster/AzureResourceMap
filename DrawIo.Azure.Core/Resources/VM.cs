namespace DrawIo.Azure.Core.Resources
{
    class VM : AzureResource
    {
        public override bool IsSpecific => true;
        public Identity? Identity { get; set; }
    }
}