using System;

namespace AzureDiagrams.Resources.Retrievers;

public class DiagramException : Exception
{
    public DiagramException(string message, Exception inner) : base(message, inner)
    {
    }
}