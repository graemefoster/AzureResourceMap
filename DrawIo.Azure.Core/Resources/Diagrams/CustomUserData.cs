using System;

namespace DrawIo.Azure.Core.Resources.Diagrams;

class CustomUserData
{
    public Func<string> Draw { get; init; }
    public string Name { get; init; }
}