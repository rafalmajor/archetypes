namespace com.softwarearchetypes.graphs.influence;

internal sealed record class InfluenceUnit(PhysicsProcess processValue, Laboratory laboratoryValue)
{
    internal PhysicsProcess process() => processValue;

    internal Laboratory laboratory() => laboratoryValue;
}