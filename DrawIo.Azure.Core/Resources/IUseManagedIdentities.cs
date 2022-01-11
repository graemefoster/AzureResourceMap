namespace DrawIo.Azure.Core.Resources;

public interface IUseManagedIdentities
{
    bool DoYouUseThisUserAssignedClientId(string clientId);
    void CreateManagedIdentityFlowBackToMe(UserAssignedManagedIdentity userAssignedManagedIdentity);
}