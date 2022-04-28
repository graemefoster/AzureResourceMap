using System;
using System.Collections.Generic;

namespace AzureDiagrams.Resources;

public static class EnumerableEx
{
    /// <summary>
    ///     Not lazy.
    /// </summary>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
        foreach (var item in items) action(item);
    }
    public static void ForEach<T>(this IEnumerable<T> items, Action<int, T> action)
    {
        var idx = 0;
        foreach (var item in items) action(idx++, item);
    }

}