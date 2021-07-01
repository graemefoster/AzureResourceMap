namespace DrawIo.Azure.Core.Resources
{
    class KeyVault : AzureResource
    {
        public override bool IsSpecific => true;
        public override string Image => "img/lib/azure2/security/Key_Vaults.svg";
    }
}