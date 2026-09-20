namespace com.softwarearchetypes.graphs.influence;

internal sealed record class Laboratory(string? nameValue)
{
    internal string? name() => nameValue;
}
