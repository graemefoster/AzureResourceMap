namespace DrawIo.Azure.Core.Resources
{
    class PIP : AzureResource
    {
        public override bool IsSpecific => true;
        public override string Image => "img/lib/azure2/networking/Public_IP_Addresses.svg";
    }
}