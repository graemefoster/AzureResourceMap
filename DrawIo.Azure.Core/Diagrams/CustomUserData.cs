using System;

namespace DrawIo.Azure.Core.Diagrams;

internal class CustomUserData
{
    public Func<string> Draw { get; init; }
    public string Name { get; init; }
}