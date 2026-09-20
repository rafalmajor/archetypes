using System.Collections.Generic;
using Xunit;
using static SoftwareArchetypes.Graphs.Influence.Fixtures;
using static SoftwareArchetypes.Graphs.Influence.InfluenceMapAssert;

namespace SoftwareArchetypes.Graphs.Influence;

public sealed class InfluenceMapTest
{
    [Fact]
    public void ShouldCreateInfluenceGraphAsCartesianProductOfPhysicsAndLaboratories()
    {
        PhysicsInfluence physics = PhysicsInfluence.CreateBuilder().AddInfluence(THERMAL, CONDUCTIVITY).Build();

        InfluenceMap influence = InfluenceMap.CreateBuilder()
            .WithPhysics(physics)
            .WithInfrastructure(EmptyInfrastructure())
            .WithLaboratories(new HashSet<Laboratory> { LAB_A, LAB_B, LAB_C })
            .Build();

        AssertThat(influence)
            .HasEdge(THERMAL, LAB_A, CONDUCTIVITY, LAB_A)
            .HasEdge(THERMAL, LAB_A, CONDUCTIVITY, LAB_B)
            .HasEdge(THERMAL, LAB_A, CONDUCTIVITY, LAB_C)
            .HasEdge(THERMAL, LAB_B, CONDUCTIVITY, LAB_A)
            .HasEdge(THERMAL, LAB_B, CONDUCTIVITY, LAB_B)
            .HasEdge(THERMAL, LAB_B, CONDUCTIVITY, LAB_C)
            .HasEdge(THERMAL, LAB_C, CONDUCTIVITY, LAB_A)
            .HasEdge(THERMAL, LAB_C, CONDUCTIVITY, LAB_B)
            .HasEdge(THERMAL, LAB_C, CONDUCTIVITY, LAB_C)
            .HasEdgeCount(9);
    }

    [Fact]
    public void ShouldCombinePhysicsCartesianProductWithInfrastructureConstraints()
    {
        PhysicsInfluence physics = PhysicsInfluence.CreateBuilder().AddInfluence(THERMAL, CONDUCTIVITY).Build();
        InfrastructureInfluence infrastructure = InfrastructureInfluence.CreateBuilder()
            .AddConstraint(SPECTROSCOPY, LAB_A, THERMAL, LAB_B)
            .Build();

        InfluenceMap influence = InfluenceMap.CreateBuilder()
            .WithPhysics(physics)
            .WithInfrastructure(infrastructure)
            .WithLaboratories(new HashSet<Laboratory> { LAB_A, LAB_B })
            .Build();

        AssertThat(influence)
            .HasEdge(THERMAL, LAB_A, CONDUCTIVITY, LAB_A)
            .HasEdge(THERMAL, LAB_A, CONDUCTIVITY, LAB_B)
            .HasEdge(THERMAL, LAB_B, CONDUCTIVITY, LAB_A)
            .HasEdge(THERMAL, LAB_B, CONDUCTIVITY, LAB_B)
            .HasEdge(SPECTROSCOPY, LAB_A, THERMAL, LAB_B)
            .HasEdgeCount(5);
    }

    [Fact]
    public void ShouldCreateInfluenceGraphBasedOnLaboratoryAdjacency()
    {
        PhysicsInfluence physics = PhysicsInfluence.CreateBuilder().AddInfluence(THERMAL, CONDUCTIVITY).Build();
        LaboratoryAdjacency adjacency = LaboratoryAdjacency.CreateBuilder()
            .Adjacent(LAB_A, LAB_B)
            .Adjacent(LAB_B, LAB_C)
            .Build();

        InfluenceMap influence = InfluenceMap.CreateBuilder()
            .WithPhysics(physics)
            .WithInfrastructure(EmptyInfrastructure())
            .WithLaboratoryAdjacency(adjacency)
            .Build();

        AssertThat(influence)
            .HasEdge(THERMAL, LAB_A, CONDUCTIVITY, LAB_B)
            .HasEdge(THERMAL, LAB_B, CONDUCTIVITY, LAB_C)
            .HasEdgeCount(2);
    }
}
