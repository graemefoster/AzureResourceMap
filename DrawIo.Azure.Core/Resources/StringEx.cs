using System;

namespace DrawIo.Azure.Core.Resources;

public static class StringEx
{
    public static string GetHostNameFromUrlString(this string urlString)
    {
        return new Uri(urlString, UriKind.RelativeOrAbsolute).Host;
    }
}