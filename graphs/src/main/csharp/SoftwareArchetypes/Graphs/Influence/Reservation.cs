namespace SoftwareArchetypes.Graphs.Influence;

internal sealed record class Reservation(PhysicsProcess processValue, Laboratory laboratoryValue)
{
    internal PhysicsProcess Process() => processValue;

    internal Laboratory Laboratory() => laboratoryValue;
}
