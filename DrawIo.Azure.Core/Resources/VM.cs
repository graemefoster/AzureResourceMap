namespace DrawIo.Azure.Core.Resources
{
    class VM : AzureResource
    {
        public override bool IsSpecific => true;
        public Identity? Identity { get; set; }
        public override string Image => "img/lib/azure2/compute/Virtual_Machine.svg";
    }
}