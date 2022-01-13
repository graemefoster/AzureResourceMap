namespace DrawIo.Azure.Core.Resources;

public class UserAssignedIdentity
{
    public string PrincipalId { get; set; } = default!;
    public string ClientId { get; set; } = default!;
}