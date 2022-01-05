using System;

namespace DrawIo.Azure.Core.Diagrams;

internal class CustomUserData
{
    public CustomUserData(Func<string> draw, string name, string id)
    {
        Draw = draw;
        Name = name;
        Id = id;
    }

    public Func<string> Draw { get; init; }
    public string Name { get; init; }
    public string Id { get; init; }
}