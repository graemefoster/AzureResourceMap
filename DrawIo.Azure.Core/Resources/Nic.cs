namespace DrawIo.Azure.Core.Resources
{
    class Nic : AzureResource
    {
        public override bool IsSpecific => true;
        public override string Image => "img/lib/azure2/networking/Network_Interfaces.svg";
    }
}