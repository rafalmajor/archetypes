namespace SoftwareArchetypes.Graphs.Influence;

internal sealed record class PhysicsProcess(string? nameValue)
{
    internal string? Name() => nameValue;
}
