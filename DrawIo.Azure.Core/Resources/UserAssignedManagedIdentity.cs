namespace DrawIo.Azure.Core.Resources
{
    class UserAssignedManagedIdentity : AzureResource
    {
        public override bool IsSpecific => true;
        public override string Image => "img/lib/mscae/Managed_Identities.svg";
    }
}