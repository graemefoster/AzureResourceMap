namespace DrawIo.Azure.Core.Resources
{
    class App : AzureResource
    {
        public override bool IsSpecific => true;
        public string Kind { get; set; }
        public Identity? Identity { get; set; }
    }
}