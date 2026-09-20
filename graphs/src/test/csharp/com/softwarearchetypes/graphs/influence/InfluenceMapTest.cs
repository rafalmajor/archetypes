using System.Collections.Generic;
using Xunit;
using static com.softwarearchetypes.graphs.influence.Fixtures;
using static com.softwarearchetypes.graphs.influence.InfluenceMapAssert;

namespace com.softwarearchetypes.graphs.influence;

public sealed class InfluenceMapTest
{
    [Fact]
    public void shouldCreateInfluenceGraphAsCartesianProductOfPhysicsAndLaboratories()
    {
        PhysicsInfluence physics = PhysicsInfluence.builder().addInfluence(THERMAL, CONDUCTIVITY).build();

        InfluenceMap influence = InfluenceMap.builder()
            .withPhysics(physics)
            .withInfrastructure(emptyInfrastructure())
            .withLaboratories(new HashSet<Laboratory> { LAB_A, LAB_B, LAB_C })
            .build();

        assertThat(influence)
            .hasEdge(THERMAL, LAB_A, CONDUCTIVITY, LAB_A)
            .hasEdge(THERMAL, LAB_A, CONDUCTIVITY, LAB_B)
            .hasEdge(THERMAL, LAB_A, CONDUCTIVITY, LAB_C)
            .hasEdge(THERMAL, LAB_B, CONDUCTIVITY, LAB_A)
            .hasEdge(THERMAL, LAB_B, CONDUCTIVITY, LAB_B)
            .hasEdge(THERMAL, LAB_B, CONDUCTIVITY, LAB_C)
            .hasEdge(THERMAL, LAB_C, CONDUCTIVITY, LAB_A)
            .hasEdge(THERMAL, LAB_C, CONDUCTIVITY, LAB_B)
            .hasEdge(THERMAL, LAB_C, CONDUCTIVITY, LAB_C)
            .hasEdgeCount(9);
    }

    [Fact]
    public void shouldCombinePhysicsCartesianProductWithInfrastructureConstraints()
    {
        PhysicsInfluence physics = PhysicsInfluence.builder().addInfluence(THERMAL, CONDUCTIVITY).build();
        InfrastructureInfluence infrastructure = InfrastructureInfluence.builder()
            .addConstraint(SPECTROSCOPY, LAB_A, THERMAL, LAB_B)
            .build();

        InfluenceMap influence = InfluenceMap.builder()
            .withPhysics(physics)
            .withInfrastructure(infrastructure)
            .withLaboratories(new HashSet<Laboratory> { LAB_A, LAB_B })
            .build();

        assertThat(influence)
            .hasEdge(THERMAL, LAB_A, CONDUCTIVITY, LAB_A)
            .hasEdge(THERMAL, LAB_A, CONDUCTIVITY, LAB_B)
            .hasEdge(THERMAL, LAB_B, CONDUCTIVITY, LAB_A)
            .hasEdge(THERMAL, LAB_B, CONDUCTIVITY, LAB_B)
            .hasEdge(SPECTROSCOPY, LAB_A, THERMAL, LAB_B)
            .hasEdgeCount(5);
    }

    [Fact]
    public void shouldCreateInfluenceGraphBasedOnLaboratoryAdjacency()
    {
        PhysicsInfluence physics = PhysicsInfluence.builder().addInfluence(THERMAL, CONDUCTIVITY).build();
        LaboratoryAdjacency adjacency = LaboratoryAdjacency.builder()
            .adjacent(LAB_A, LAB_B)
            .adjacent(LAB_B, LAB_C)
            .build();

        InfluenceMap influence = InfluenceMap.builder()
            .withPhysics(physics)
            .withInfrastructure(emptyInfrastructure())
            .withLaboratoryAdjacency(adjacency)
            .build();

        assertThat(influence)
            .hasEdge(THERMAL, LAB_A, CONDUCTIVITY, LAB_B)
            .hasEdge(THERMAL, LAB_B, CONDUCTIVITY, LAB_C)
            .hasEdgeCount(2);
    }
}
