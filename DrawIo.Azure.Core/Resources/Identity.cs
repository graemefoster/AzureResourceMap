﻿using System.Collections.Generic;

namespace DrawIo.Azure.Core.Resources;

public class Identity
{
    public string? PrincipalId { get; set; }
    public string? TenantId { get; set; }
    public string Type { get; set; } = default!;
    public Dictionary<string, UserAssignedIdentity>? UserAssignedIdentities { get; set; }
}

public class UserAssignedIdentity
{
    public string PrincipalId { get; set; } = default!;
    public string ClientId { get; set; } = default!;
}