using System;
using System.Collections.Generic;

namespace com.softwarearchetypes.common;

public static class CollectionTransformations
{
    public static Dictionary<string, string?> keyValueMapFrom(string?[]? parameters)
    {
        Dictionary<string, string?> result = [];
        if (parameters is null)
        {
            return result;
        }

        if (parameters.Length % 2 != 0)
        {
            throw new ArgumentException("The number of arguments must be even (key, productName, ...)");
        }

        for (int index = 0; index < parameters.Length; index += 2)
        {
            string? key = parameters[index];
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Key (idx={index}) cannot be empty or null");
            }

            result[key] = parameters[index + 1];
        }

        return result;
    }

    public static ISet<T> subtract<T>(ISet<T> minuend, ISet<T> subtrahend)
    {
        HashSet<T> result = new(minuend);
        result.ExceptWith(subtrahend);
        return result;
    }
}
