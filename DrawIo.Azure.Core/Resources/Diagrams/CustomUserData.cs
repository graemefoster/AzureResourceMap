using System;

namespace DrawIo.Azure.Core.Resources;

class CustomUserData
{
    public Func<string> Draw { get; init; }
    public string Name { get; init; }
}