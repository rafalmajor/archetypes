using System;

namespace com.softwarearchetypes.graphs.cycles;

internal sealed record class OwnerId(string? valueValue)
{
    internal string? value() => valueValue;

    internal static OwnerId of(string? value) => new(value);

    internal static OwnerId empty() => new(string.Empty);

    internal bool isEmpty() => string.IsNullOrEmpty(valueValue);
}