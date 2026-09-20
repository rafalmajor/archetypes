using System;
using System.Diagnostics.CodeAnalysis;

namespace com.softwarearchetypes.common;

public static class Preconditions
{
    public static void checkArgument(bool expression, string errorMessage)
    {
        if (!expression)
        {
            throw new ArgumentException(errorMessage);
        }
    }

    public static void checkState(bool state, string errorMessage)
    {
        if (!state)
        {
            throw new InvalidOperationException(errorMessage);
        }
    }

    public static void checkNotNull([NotNull] object? value, string errorMessage)
    {
        if (value is null)
        {
            throw new ArgumentException(errorMessage);
        }
    }
}
