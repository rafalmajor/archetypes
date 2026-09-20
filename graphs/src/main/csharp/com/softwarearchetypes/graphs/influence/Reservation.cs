namespace com.softwarearchetypes.graphs.influence;

internal sealed record class Reservation(PhysicsProcess processValue, Laboratory laboratoryValue)
{
    internal PhysicsProcess process() => processValue;

    internal Laboratory laboratory() => laboratoryValue;
}