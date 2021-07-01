namespace DrawIo.Azure.Core.Resources
{
    class ASP : AzureResource
    {
        public override bool IsSpecific => true;
        public string Kind { get; set; }
    }
}