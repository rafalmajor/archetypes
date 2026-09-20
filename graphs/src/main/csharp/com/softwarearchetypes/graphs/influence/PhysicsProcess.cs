namespace com.softwarearchetypes.graphs.influence;

internal sealed record class PhysicsProcess(string? nameValue)
{
    internal string? name() => nameValue;
}
