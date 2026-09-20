namespace com.softwarearchetypes.graphs.cycles;

internal sealed record class SlotId(string? valueValue)
{
    internal string? value() => valueValue;

    internal static SlotId of(string? value) => new(value);
}
