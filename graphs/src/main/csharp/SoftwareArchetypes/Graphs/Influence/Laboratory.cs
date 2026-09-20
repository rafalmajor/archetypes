namespace SoftwareArchetypes.Graphs.Influence;

internal sealed record class Laboratory(string? nameValue)
{
    internal string? Name() => nameValue;
}
