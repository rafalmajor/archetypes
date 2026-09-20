namespace SoftwareArchetypes.Graphs.Influence;

internal sealed record class InfluenceUnit(PhysicsProcess processValue, Laboratory laboratoryValue)
{
    internal PhysicsProcess Process() => processValue;

    internal Laboratory Laboratory() => laboratoryValue;
}
