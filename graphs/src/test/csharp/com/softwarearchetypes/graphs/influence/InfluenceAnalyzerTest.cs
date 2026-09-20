using System.Collections.Generic;
using Xunit;
using static com.softwarearchetypes.graphs.influence.Fixtures;

namespace com.softwarearchetypes.graphs.influence;

public sealed class InfluenceAnalyzerTest
{
    [Fact]
    public void singleDirectConflict()
    {
        PhysicsInfluence physics = PhysicsInfluence.builder().addInfluence(THERMAL, CONDUCTIVITY).build();
        InfluenceMap influenceMap = mapFor(physics, LAB_A, LAB_B);
        Reservation existing = new(CONDUCTIVITY, LAB_B);
        Reservation newReservation = new(THERMAL, LAB_A);

        int conflicts = new InfluanceAnalyzer(influenceMap)
            .countConflicts(newReservation, new HashSet<Reservation> { existing });

        Assert.Equal(1, conflicts);
    }

    [Fact]
    public void multipleDirectConflicts()
    {
        PhysicsInfluence physics = PhysicsInfluence.builder()
            .addInfluence(THERMAL, CONDUCTIVITY)
            .addInfluence(THERMAL, SPECTROSCOPY)
            .build();
        InfluenceMap influenceMap = mapFor(physics, LAB_A, LAB_B, LAB_C);
        Reservation existing1 = new(CONDUCTIVITY, LAB_B);
        Reservation existing2 = new(SPECTROSCOPY, LAB_C);

        int conflicts = new InfluanceAnalyzer(influenceMap).countConflicts(
            new Reservation(THERMAL, LAB_A),
            new HashSet<Reservation> { existing1, existing2 });

        Assert.Equal(2, conflicts);
    }

    [Fact]
    public void noDirectConflictsWhenNoInfluence()
    {
        InfluenceMap influenceMap = mapFor(PhysicsInfluence.builder().build(), LAB_A, LAB_B);

        int conflicts = new InfluanceAnalyzer(influenceMap).countConflicts(
            new Reservation(THERMAL, LAB_A),
            new HashSet<Reservation> { new(SPECTROSCOPY, LAB_B) });

        Assert.Equal(0, conflicts);
    }

    [Fact]
    public void newReservationMergesTwoInfluenceZonesIntoOne()
    {
        PhysicsProcess processA = new("A");
        PhysicsProcess processB = new("B");
        PhysicsProcess processX = new("X");
        PhysicsProcess processC = new("C");
        PhysicsProcess processD = new("D");
        PhysicsInfluence physics = PhysicsInfluence.builder()
            .addInfluence(processA, processB)
            .addInfluence(processC, processD)
            .addInfluence(processB, processX)
            .addInfluence(processX, processC)
            .build();
        LaboratoryAdjacency adjacency = LaboratoryAdjacency.builder()
            .adjacent(LAB_A, LAB_A)
            .adjacent(LAB_B, LAB_B)
            .adjacent(LAB_A, LAB_C)
            .adjacent(LAB_C, LAB_B)
            .build();
        InfluenceMap influenceMap = InfluenceMap.builder()
            .withPhysics(physics)
            .withInfrastructure(emptyInfrastructure())
            .withLaboratoryAdjacency(adjacency)
            .build();
        Reservation existing1 = new(processA, LAB_A);
        Reservation existing2 = new(processB, LAB_A);
        Reservation existing3 = new(processC, LAB_B);
        Reservation existing4 = new(processD, LAB_B);
        Reservation newReservation = new(processX, LAB_C);
        HashSet<Reservation> existing = [existing1, existing2, existing3, existing4];
        HashSet<Reservation> all = [existing1, existing2, existing3, existing4, newReservation];

        ISet<InfluenceZone> zonesBefore = new InfluanceAnalyzer(influenceMap).analyzeInfluenceZones(existing);
        InfluenceZone zoneAfter = new InfluanceAnalyzer(influenceMap).findInfluenceZone(newReservation, all);

        Assert.Equal(2, zonesBefore.Count);
        Assert.Equal(5, zoneAfter.size());
        Assert.Equal(4, zoneAfter.countReservationsToNegotiateWith(newReservation));
        Assert.True(new HashSet<Reservation> { existing1, existing2, existing3, existing4 }
            .SetEquals(zoneAfter.getReservationsToNegotiateWith(newReservation)));
    }

    [Fact]
    public void newReservationAddsThirdIndependentInfluenceZone()
    {
        PhysicsProcess processA = new("A");
        PhysicsProcess processB = new("B");
        PhysicsProcess processC = new("C");
        PhysicsProcess processD = new("D");
        PhysicsProcess processE = new("E");
        PhysicsInfluence physics = PhysicsInfluence.builder()
            .addInfluence(processA, processB)
            .addInfluence(processC, processD)
            .build();
        InfluenceMap influenceMap = mapFor(physics, LAB_A, LAB_B, LAB_C);
        Reservation existing1 = new(processA, LAB_A);
        Reservation existing2 = new(processB, LAB_A);
        Reservation existing3 = new(processC, LAB_B);
        Reservation existing4 = new(processD, LAB_B);
        Reservation newReservation = new(processE, LAB_C);
        HashSet<Reservation> existing = [existing1, existing2, existing3, existing4];
        HashSet<Reservation> all = [existing1, existing2, existing3, existing4, newReservation];
        InfluanceAnalyzer analyzer = new(influenceMap);

        ISet<InfluenceZone> zonesBefore = analyzer.analyzeInfluenceZones(existing);
        ISet<InfluenceZone> zonesAfter = analyzer.analyzeInfluenceZones(all);
        InfluenceZone independentZone = analyzer.findInfluenceZone(newReservation, all);

        Assert.Equal(2, zonesBefore.Count);
        Assert.Equal(3, zonesAfter.Count);
        Assert.Equal(0, independentZone.countReservationsToNegotiateWith(newReservation));
        Assert.Equal(1, independentZone.size());
    }

    private static InfluenceMap mapFor(PhysicsInfluence physics, params Laboratory[] laboratories) =>
        InfluenceMap.builder()
            .withPhysics(physics)
            .withInfrastructure(emptyInfrastructure())
            .withLaboratories(new HashSet<Laboratory>(laboratories))
            .build();
}
