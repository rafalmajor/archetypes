using System.Collections.Generic;
using Xunit;
using static SoftwareArchetypes.Graphs.Influence.Fixtures;

namespace SoftwareArchetypes.Graphs.Influence;

public sealed class InfluenceAnalyzerTest
{
    [Fact]
    public void SingleDirectConflict()
    {
        PhysicsInfluence physics = PhysicsInfluence.CreateBuilder().AddInfluence(THERMAL, CONDUCTIVITY).Build();
        InfluenceMap influenceMap = MapFor(physics, LAB_A, LAB_B);
        Reservation existing = new(CONDUCTIVITY, LAB_B);
        Reservation newReservation = new(THERMAL, LAB_A);

        int conflicts = new InfluanceAnalyzer(influenceMap)
            .CountConflicts(newReservation, new HashSet<Reservation> { existing });

        Assert.Equal(1, conflicts);
    }

    [Fact]
    public void MultipleDirectConflicts()
    {
        PhysicsInfluence physics = PhysicsInfluence.CreateBuilder()
            .AddInfluence(THERMAL, CONDUCTIVITY)
            .AddInfluence(THERMAL, SPECTROSCOPY)
            .Build();
        InfluenceMap influenceMap = MapFor(physics, LAB_A, LAB_B, LAB_C);
        Reservation existing1 = new(CONDUCTIVITY, LAB_B);
        Reservation existing2 = new(SPECTROSCOPY, LAB_C);

        int conflicts = new InfluanceAnalyzer(influenceMap).CountConflicts(
            new Reservation(THERMAL, LAB_A),
            new HashSet<Reservation> { existing1, existing2 });

        Assert.Equal(2, conflicts);
    }

    [Fact]
    public void NoDirectConflictsWhenNoInfluence()
    {
        InfluenceMap influenceMap = MapFor(PhysicsInfluence.CreateBuilder().Build(), LAB_A, LAB_B);

        int conflicts = new InfluanceAnalyzer(influenceMap).CountConflicts(
            new Reservation(THERMAL, LAB_A),
            new HashSet<Reservation> { new(SPECTROSCOPY, LAB_B) });

        Assert.Equal(0, conflicts);
    }

    [Fact]
    public void NewReservationMergesTwoInfluenceZonesIntoOne()
    {
        PhysicsProcess processA = new("A");
        PhysicsProcess processB = new("B");
        PhysicsProcess processX = new("X");
        PhysicsProcess processC = new("C");
        PhysicsProcess processD = new("D");
        PhysicsInfluence physics = PhysicsInfluence.CreateBuilder()
            .AddInfluence(processA, processB)
            .AddInfluence(processC, processD)
            .AddInfluence(processB, processX)
            .AddInfluence(processX, processC)
            .Build();
        LaboratoryAdjacency adjacency = LaboratoryAdjacency.CreateBuilder()
            .Adjacent(LAB_A, LAB_A)
            .Adjacent(LAB_B, LAB_B)
            .Adjacent(LAB_A, LAB_C)
            .Adjacent(LAB_C, LAB_B)
            .Build();
        InfluenceMap influenceMap = InfluenceMap.CreateBuilder()
            .WithPhysics(physics)
            .WithInfrastructure(EmptyInfrastructure())
            .WithLaboratoryAdjacency(adjacency)
            .Build();
        Reservation existing1 = new(processA, LAB_A);
        Reservation existing2 = new(processB, LAB_A);
        Reservation existing3 = new(processC, LAB_B);
        Reservation existing4 = new(processD, LAB_B);
        Reservation newReservation = new(processX, LAB_C);
        HashSet<Reservation> existing = [existing1, existing2, existing3, existing4];
        HashSet<Reservation> all = [existing1, existing2, existing3, existing4, newReservation];

        ISet<InfluenceZone> zonesBefore = new InfluanceAnalyzer(influenceMap).AnalyzeInfluenceZones(existing);
        InfluenceZone zoneAfter = new InfluanceAnalyzer(influenceMap).FindInfluenceZone(newReservation, all);

        Assert.Equal(2, zonesBefore.Count);
        Assert.Equal(5, zoneAfter.Size());
        Assert.Equal(4, zoneAfter.CountReservationsToNegotiateWith(newReservation));
        Assert.True(new HashSet<Reservation> { existing1, existing2, existing3, existing4 }
            .SetEquals(zoneAfter.GetReservationsToNegotiateWith(newReservation)));
    }

    [Fact]
    public void NewReservationAddsThirdIndependentInfluenceZone()
    {
        PhysicsProcess processA = new("A");
        PhysicsProcess processB = new("B");
        PhysicsProcess processC = new("C");
        PhysicsProcess processD = new("D");
        PhysicsProcess processE = new("E");
        PhysicsInfluence physics = PhysicsInfluence.CreateBuilder()
            .AddInfluence(processA, processB)
            .AddInfluence(processC, processD)
            .Build();
        InfluenceMap influenceMap = MapFor(physics, LAB_A, LAB_B, LAB_C);
        Reservation existing1 = new(processA, LAB_A);
        Reservation existing2 = new(processB, LAB_A);
        Reservation existing3 = new(processC, LAB_B);
        Reservation existing4 = new(processD, LAB_B);
        Reservation newReservation = new(processE, LAB_C);
        HashSet<Reservation> existing = [existing1, existing2, existing3, existing4];
        HashSet<Reservation> all = [existing1, existing2, existing3, existing4, newReservation];
        InfluanceAnalyzer analyzer = new(influenceMap);

        ISet<InfluenceZone> zonesBefore = analyzer.AnalyzeInfluenceZones(existing);
        ISet<InfluenceZone> zonesAfter = analyzer.AnalyzeInfluenceZones(all);
        InfluenceZone independentZone = analyzer.FindInfluenceZone(newReservation, all);

        Assert.Equal(2, zonesBefore.Count);
        Assert.Equal(3, zonesAfter.Count);
        Assert.Equal(0, independentZone.CountReservationsToNegotiateWith(newReservation));
        Assert.Equal(1, independentZone.Size());
    }

    private static InfluenceMap MapFor(PhysicsInfluence physics, params Laboratory[] laboratories) =>
        InfluenceMap.CreateBuilder()
            .WithPhysics(physics)
            .WithInfrastructure(EmptyInfrastructure())
            .WithLaboratories(new HashSet<Laboratory>(laboratories))
            .Build();
}
