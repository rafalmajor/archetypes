using System;

namespace SoftwareArchetypes.Graphs.Cycles;

internal sealed record class OwnerId(string? valueValue)
{
    internal string? Value() => valueValue;

    internal static OwnerId Of(string? value) => new(value);

    internal static OwnerId Empty() => new(string.Empty);

    internal bool IsEmpty() => string.IsNullOrEmpty(valueValue);
}
