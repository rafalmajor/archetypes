namespace com.softwarearchetypes.graphs.influence;

internal static class Fixtures
{
    internal static readonly PhysicsProcess THERMAL = new("thermal");
    internal static readonly PhysicsProcess CONDUCTIVITY = new("conductivity");
    internal static readonly PhysicsProcess SPECTROSCOPY = new("spectroscopy");
    internal static readonly Laboratory LAB_A = new("Lab A");
    internal static readonly Laboratory LAB_B = new("Lab B");
    internal static readonly Laboratory LAB_C = new("Lab C");

    internal static InfrastructureInfluence emptyInfrastructure() =>
        InfrastructureInfluence.builder().build();
}
