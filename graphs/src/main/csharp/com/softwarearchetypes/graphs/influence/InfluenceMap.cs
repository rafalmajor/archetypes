using System;
using System.Collections.Generic;

namespace com.softwarearchetypes.graphs.influence;

internal sealed class InfluenceMap
{
    private readonly DirectedGraph<InfluenceUnit> graph;

    private InfluenceMap(DirectedGraph<InfluenceUnit> graph)
    {
        this.graph = graph;
    }

    internal static InfluenceMap of(
        PhysicsInfluence physicsInfluence,
        InfrastructureInfluence infrastructureInfluence,
        ISet<Laboratory> laboratories)
    {
        DirectedGraph<InfluenceUnit> result = cartesianOf(physicsInfluence, laboratories);
        addInfrastructure(infrastructureInfluence, result);
        return new InfluenceMap(result);
    }

    internal static InfluenceMap of(
        PhysicsInfluence physicsInfluence,
        InfrastructureInfluence infrastructureInfluence,
        LaboratoryAdjacency laboratoryAdjacency)
    {
        DirectedGraph<InfluenceUnit> result = adjacencyOf(physicsInfluence, laboratoryAdjacency);
        addInfrastructure(infrastructureInfluence, result);
        return new InfluenceMap(result);
    }

    internal DirectedGraph<InfluenceUnit> asGraph() => graph;

    internal bool influences(
        PhysicsProcess fromProcess,
        Laboratory fromLab,
        PhysicsProcess toProcess,
        Laboratory toLab) =>
        graph.containsEdge(
            new InfluenceUnit(fromProcess, fromLab),
            new InfluenceUnit(toProcess, toLab));

    internal bool influences(Reservation from, Reservation to) =>
        influences(from.process(), from.laboratory(), to.process(), to.laboratory());

    internal static Builder builder() => new();

    private static void addInfrastructure(
        InfrastructureInfluence infrastructureInfluence,
        DirectedGraph<InfluenceUnit> result)
    {
        foreach (DirectedEdge<InfluenceUnit> edge in infrastructureInfluence.asGraph().edges())
        {
            result.addEdge(edge.source, edge.target);
        }
    }

    private static DirectedGraph<InfluenceUnit> cartesianOf(
        PhysicsInfluence physicsInfluence,
        ISet<Laboratory> laboratories)
    {
        DirectedGraph<InfluenceUnit> result = new();
        foreach (DirectedEdge<PhysicsProcess> edge in physicsInfluence.asGraph().edges())
        {
            foreach (Laboratory fromLab in laboratories)
            {
                foreach (Laboratory toLab in laboratories)
                {
                    result.addEdge(
                        new InfluenceUnit(edge.source, fromLab),
                        new InfluenceUnit(edge.target, toLab));
                }
            }
        }

        return result;
    }

    private static DirectedGraph<InfluenceUnit> adjacencyOf(
        PhysicsInfluence physicsInfluence,
        LaboratoryAdjacency laboratoryAdjacency)
    {
        DirectedGraph<InfluenceUnit> result = new();
        foreach (DirectedEdge<PhysicsProcess> physicsEdge in physicsInfluence.asGraph().edges())
        {
            foreach (DirectedEdge<Laboratory> adjacencyEdge in laboratoryAdjacency.asGraph().edges())
            {
                result.addEdge(
                    new InfluenceUnit(physicsEdge.source, adjacencyEdge.source),
                    new InfluenceUnit(physicsEdge.target, adjacencyEdge.target));
            }
        }

        return result;
    }

    internal sealed class Builder
    {
        private PhysicsInfluence? physicsInfluence;
        private InfrastructureInfluence infrastructureInfluence = InfrastructureInfluence.builder().build();
        private ISet<Laboratory>? laboratories;
        private LaboratoryAdjacency? laboratoryAdjacency;

        internal Builder withPhysics(PhysicsInfluence value)
        {
            physicsInfluence = value;
            return this;
        }

        internal Builder withInfrastructure(InfrastructureInfluence value)
        {
            infrastructureInfluence = value;
            return this;
        }

        internal Builder withLaboratories(ISet<Laboratory> value)
        {
            laboratories = value;
            return this;
        }

        internal Builder withLaboratoryAdjacency(LaboratoryAdjacency value)
        {
            laboratoryAdjacency = value;
            return this;
        }

        internal InfluenceMap build()
        {
            PhysicsInfluence physics = physicsInfluence ?? throw new NullReferenceException();
            return laboratoryAdjacency is not null
                ? of(physics, infrastructureInfluence, laboratoryAdjacency)
                : of(physics, infrastructureInfluence, laboratories ?? throw new NullReferenceException());
        }
    }
}
