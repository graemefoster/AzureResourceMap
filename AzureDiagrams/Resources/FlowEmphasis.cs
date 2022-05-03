using System;

namespace AzureDiagrams.Resources;

[Flags]
public enum Plane
{
    Diagnostics = 1,
    Runtime = 2,
    Identity = 4,
    Inferred = 8,
    All = Diagnostics | Runtime | Identity | Inferred,
    None = 0
}