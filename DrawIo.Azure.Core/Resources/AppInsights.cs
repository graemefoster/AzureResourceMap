namespace DrawIo.Azure.Core.Resources
{
    class AppInsights : AzureResource
    {
        public override bool IsSpecific => true;
        public string Kind { get; set; }
    }
}