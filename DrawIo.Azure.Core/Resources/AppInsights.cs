namespace DrawIo.Azure.Core.Resources
{
    class AppInsights : AzureResource
    {
        public override bool IsSpecific => true;
        public string Kind { get; set; }
        public override string Image => "img/lib/azure2/devops/Application_Insights.svg";
    }
}