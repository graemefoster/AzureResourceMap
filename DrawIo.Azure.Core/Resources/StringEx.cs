using System;

namespace DrawIo.Azure.Core.Resources;

public static class StringEx
{
    public static string GetHostNameFromUrlString(this string urlString)
    {
        return new Uri(urlString, UriKind.RelativeOrAbsolute).Host;
    }

    public static string? GetHostNameFromUrlStringOrNull(this string urlString)
    {
        if (Uri.TryCreate(urlString, UriKind.Absolute, out var url)) return url.Host;

        return null;
    }
}