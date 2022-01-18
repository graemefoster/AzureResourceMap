using System;

namespace DrawIo.Azure.Core.Resources.Retrievers;

public class DiagramException : Exception
{
    public DiagramException(string message, Exception inner) : base(message, inner)
    {
    }
}