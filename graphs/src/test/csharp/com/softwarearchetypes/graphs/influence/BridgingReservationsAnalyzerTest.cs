using System.Collections.Generic;
using Xunit;
using static com.softwarearchetypes.graphs.influence.Fixtures;

namespace com.softwarearchetypes.graphs.influence;

public sealed class BridgingReservationsAnalyzerTest
{
    internal static readonly PhysicsProcess PROCESS_A = new("A");
    internal static readonly PhysicsProcess PROCESS_B = new("B");
    internal static readonly PhysicsProcess PROCESS_C = new("D");
    internal static readonly PhysicsProcess PROCESS_D = new("C");

    [Fact]
    public void independentReservationsAreNotCritical()
    {
        PhysicsProcess processA = new("A");
        PhysicsProcess processB = new("B");
        PhysicsProcess processC = new("C");
        PhysicsInfluence physics = PhysicsInfluence.builder().addInfluence(processA, processB).build();
        Reservation r1 = new(processA, LAB_A);
        Reservation r2 = new(processB, LAB_B);
        Reservation r3 = new(processC, LAB_C);

        BridgingReservations bridging = analyzerFor(physics)
            .identifyCriticalReservations(new HashSet<Reservation> { r1, r2, r3 });

        Assert.True(bridging.isEmpty());
    }

    [Fact]
    public void reservationConnectingTwoGroupsIsCritical()
    {
        PhysicsProcess processA = new("A");
        PhysicsProcess processB = new("B");
        PhysicsProcess processC = new("C");
        PhysicsProcess processX = new("X");
        PhysicsProcess processD = new("D");
        PhysicsProcess processE = new("E");
        PhysicsProcess processF = new("F");
        PhysicsInfluence physics = PhysicsInfluence.builder()
            .addInfluence(processA, processB)
            .addInfluence(processB, processC)
            .addInfluence(processC, processA)
            .addInfluence(processA, processX)
            .addInfluence(processX, processD)
            .addInfluence(processD, processE)
            .addInfluence(processE, processF)
            .addInfluence(processF, processD)
            .build();
        Reservation r1 = new(processA, LAB_A);
        Reservation r2 = new(processB, LAB_A);
        Reservation r3 = new(processC, LAB_A);
        Reservation r4 = new(processX, LAB_B);
        Reservation r5 = new(processD, LAB_C);
        Reservation r6 = new(processE, LAB_C);
        Reservation r7 = new(processF, LAB_C);

        BridgingReservations bridging = analyzerFor(physics).identifyCriticalReservations(
            new HashSet<Reservation> { r1, r2, r3, r4, r5, r6, r7 });

        Assert.Equal(3, bridging.count());
        Assert.True(bridging.isBridging(r4));
        Assert.True(bridging.isBridging(r1));
        Assert.True(bridging.isBridging(r5));
    }

    [Fact]
    public void fullyConnectedGroupHasNoCriticalReservations()
    {
        PhysicsProcess processA = new("A");
        PhysicsProcess processB = new("B");
        PhysicsProcess processC = new("C");
        PhysicsInfluence physics = PhysicsInfluence.builder()
            .addInfluence(processA, processB)
            .addInfluence(processB, processC)
            .addInfluence(processC, processA)
            .build();
        Reservation r1 = new(processA, LAB_A);
        Reservation r2 = new(processB, LAB_B);
        Reservation r3 = new(processC, LAB_C);

        BridgingReservations bridging = analyzerFor(physics)
            .identifyCriticalReservations(new HashSet<Reservation> { r1, r2, r3 });

        Assert.True(bridging.isEmpty());
        Assert.Equal(0, bridging.count());
    }

    [Fact]
    public void longChainHasMultipleCriticalReservations()
    {
        PhysicsInfluence physics = PhysicsInfluence.builder()
            .addInfluence(PROCESS_A, PROCESS_B)
            .addInfluence(PROCESS_B, PROCESS_C)
            .addInfluence(PROCESS_C, PROCESS_D)
            .build();
        Reservation r1 = new(PROCESS_A, LAB_A);
        Reservation r2 = new(PROCESS_B, LAB_B);
        Reservation r3 = new(PROCESS_C, LAB_C);
        Reservation r4 = new(PROCESS_D, LAB_A);

        BridgingReservations bridging = analyzerFor(physics)
            .identifyCriticalReservations(new HashSet<Reservation> { r1, r2, r3, r4 });

        Assert.Equal(2, bridging.count());
        Assert.True(bridging.isBridging(r2));
        Assert.True(bridging.isBridging(r3));
    }

    [Fact]
    public void emptySetHasNoCriticalReservations()
    {
        BridgingReservations bridging = analyzerFor(PhysicsInfluence.builder().build())
            .identifyCriticalReservations(new HashSet<Reservation>());

        Assert.True(bridging.isEmpty());
        Assert.Equal(0, bridging.count());
    }

    private static InfluanceAnalyzer analyzerFor(PhysicsInfluence physics)
    {
        InfluenceMap map = InfluenceMap.builder()
            .withPhysics(physics)
            .withLaboratories(new HashSet<Laboratory> { LAB_A, LAB_B, LAB_C })
            .build();
        return new InfluanceAnalyzer(map);
    }
}
