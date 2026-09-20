using System.Collections.Generic;
using Xunit;
using static SoftwareArchetypes.Graphs.Influence.Fixtures;

namespace SoftwareArchetypes.Graphs.Influence;

public sealed class BridgingReservationsAnalyzerTest
{
    internal static readonly PhysicsProcess PROCESS_A = new("A");
    internal static readonly PhysicsProcess PROCESS_B = new("B");
    internal static readonly PhysicsProcess PROCESS_C = new("D");
    internal static readonly PhysicsProcess PROCESS_D = new("C");

    [Fact]
    public void IndependentReservationsAreNotCritical()
    {
        PhysicsProcess processA = new("A");
        PhysicsProcess processB = new("B");
        PhysicsProcess processC = new("C");
        PhysicsInfluence physics = PhysicsInfluence.CreateBuilder().AddInfluence(processA, processB).Build();
        Reservation r1 = new(processA, LAB_A);
        Reservation r2 = new(processB, LAB_B);
        Reservation r3 = new(processC, LAB_C);

        BridgingReservations bridging = AnalyzerFor(physics)
            .IdentifyCriticalReservations(new HashSet<Reservation> { r1, r2, r3 });

        Assert.True(bridging.IsEmpty());
    }

    [Fact]
    public void ReservationConnectingTwoGroupsIsCritical()
    {
        PhysicsProcess processA = new("A");
        PhysicsProcess processB = new("B");
        PhysicsProcess processC = new("C");
        PhysicsProcess processX = new("X");
        PhysicsProcess processD = new("D");
        PhysicsProcess processE = new("E");
        PhysicsProcess processF = new("F");
        PhysicsInfluence physics = PhysicsInfluence.CreateBuilder()
            .AddInfluence(processA, processB)
            .AddInfluence(processB, processC)
            .AddInfluence(processC, processA)
            .AddInfluence(processA, processX)
            .AddInfluence(processX, processD)
            .AddInfluence(processD, processE)
            .AddInfluence(processE, processF)
            .AddInfluence(processF, processD)
            .Build();
        Reservation r1 = new(processA, LAB_A);
        Reservation r2 = new(processB, LAB_A);
        Reservation r3 = new(processC, LAB_A);
        Reservation r4 = new(processX, LAB_B);
        Reservation r5 = new(processD, LAB_C);
        Reservation r6 = new(processE, LAB_C);
        Reservation r7 = new(processF, LAB_C);

        BridgingReservations bridging = AnalyzerFor(physics).IdentifyCriticalReservations(
            new HashSet<Reservation> { r1, r2, r3, r4, r5, r6, r7 });

        Assert.Equal(3, bridging.Count());
        Assert.True(bridging.IsBridging(r4));
        Assert.True(bridging.IsBridging(r1));
        Assert.True(bridging.IsBridging(r5));
    }

    [Fact]
    public void FullyConnectedGroupHasNoCriticalReservations()
    {
        PhysicsProcess processA = new("A");
        PhysicsProcess processB = new("B");
        PhysicsProcess processC = new("C");
        PhysicsInfluence physics = PhysicsInfluence.CreateBuilder()
            .AddInfluence(processA, processB)
            .AddInfluence(processB, processC)
            .AddInfluence(processC, processA)
            .Build();
        Reservation r1 = new(processA, LAB_A);
        Reservation r2 = new(processB, LAB_B);
        Reservation r3 = new(processC, LAB_C);

        BridgingReservations bridging = AnalyzerFor(physics)
            .IdentifyCriticalReservations(new HashSet<Reservation> { r1, r2, r3 });

        Assert.True(bridging.IsEmpty());
        Assert.Equal(0, bridging.Count());
    }

    [Fact]
    public void LongChainHasMultipleCriticalReservations()
    {
        PhysicsInfluence physics = PhysicsInfluence.CreateBuilder()
            .AddInfluence(PROCESS_A, PROCESS_B)
            .AddInfluence(PROCESS_B, PROCESS_C)
            .AddInfluence(PROCESS_C, PROCESS_D)
            .Build();
        Reservation r1 = new(PROCESS_A, LAB_A);
        Reservation r2 = new(PROCESS_B, LAB_B);
        Reservation r3 = new(PROCESS_C, LAB_C);
        Reservation r4 = new(PROCESS_D, LAB_A);

        BridgingReservations bridging = AnalyzerFor(physics)
            .IdentifyCriticalReservations(new HashSet<Reservation> { r1, r2, r3, r4 });

        Assert.Equal(2, bridging.Count());
        Assert.True(bridging.IsBridging(r2));
        Assert.True(bridging.IsBridging(r3));
    }

    [Fact]
    public void EmptySetHasNoCriticalReservations()
    {
        BridgingReservations bridging = AnalyzerFor(PhysicsInfluence.CreateBuilder().Build())
            .IdentifyCriticalReservations(new HashSet<Reservation>());

        Assert.True(bridging.IsEmpty());
        Assert.Equal(0, bridging.Count());
    }

    private static InfluanceAnalyzer AnalyzerFor(PhysicsInfluence physics)
    {
        InfluenceMap map = InfluenceMap.CreateBuilder()
            .WithPhysics(physics)
            .WithLaboratories(new HashSet<Laboratory> { LAB_A, LAB_B, LAB_C })
            .Build();
        return new InfluanceAnalyzer(map);
    }
}
