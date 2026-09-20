namespace SoftwareArchetypes.Graphs.Cycles;

internal sealed record class SlotId(string? valueValue)
{
    internal string? Value() => valueValue;

    internal static SlotId Of(string? value) => new(value);
}
