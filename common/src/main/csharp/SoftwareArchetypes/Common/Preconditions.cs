using System;
using System.Diagnostics.CodeAnalysis;

namespace SoftwareArchetypes.Common;

public static class Preconditions
{
    public static void CheckArgument(bool expression, string errorMessage)
    {
        if (!expression)
        {
            throw new ArgumentException(errorMessage);
        }
    }

    public static void CheckState(bool state, string errorMessage)
    {
        if (!state)
        {
            throw new InvalidOperationException(errorMessage);
        }
    }

    public static void CheckNotNull([NotNull] object? value, string errorMessage)
    {
        if (value is null)
        {
            throw new ArgumentException(errorMessage);
        }
    }
}
