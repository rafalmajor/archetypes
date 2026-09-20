using System;
using System.Collections.Generic;

namespace SoftwareArchetypes.Graphs.Influence;

internal sealed class InfluenceMap
{
    private readonly DirectedGraph<InfluenceUnit> graph;

    private InfluenceMap(DirectedGraph<InfluenceUnit> graph)
    {
        this.graph = graph;
    }

    internal static InfluenceMap Of(
        PhysicsInfluence physicsInfluence,
        InfrastructureInfluence infrastructureInfluence,
        ISet<Laboratory> laboratories)
    {
        DirectedGraph<InfluenceUnit> result = CartesianOf(physicsInfluence, laboratories);
        AddInfrastructure(infrastructureInfluence, result);
        return new InfluenceMap(result);
    }

    internal static InfluenceMap Of(
        PhysicsInfluence physicsInfluence,
        InfrastructureInfluence infrastructureInfluence,
        LaboratoryAdjacency laboratoryAdjacency)
    {
        DirectedGraph<InfluenceUnit> result = AdjacencyOf(physicsInfluence, laboratoryAdjacency);
        AddInfrastructure(infrastructureInfluence, result);
        return new InfluenceMap(result);
    }

    internal DirectedGraph<InfluenceUnit> AsGraph() => graph;

    internal bool Influences(
        PhysicsProcess fromProcess,
        Laboratory fromLab,
        PhysicsProcess toProcess,
        Laboratory toLab) =>
        graph.ContainsEdge(
            new InfluenceUnit(fromProcess, fromLab),
            new InfluenceUnit(toProcess, toLab));

    internal bool Influences(Reservation from, Reservation to) =>
        Influences(from.Process(), from.Laboratory(), to.Process(), to.Laboratory());

    internal static Builder CreateBuilder() => new();

    private static void AddInfrastructure(
        InfrastructureInfluence infrastructureInfluence,
        DirectedGraph<InfluenceUnit> result)
    {
        foreach (DirectedEdge<InfluenceUnit> edge in infrastructureInfluence.AsGraph().Edges())
        {
            result.AddEdge(edge.source, edge.target);
        }
    }

    private static DirectedGraph<InfluenceUnit> CartesianOf(
        PhysicsInfluence physicsInfluence,
        ISet<Laboratory> laboratories)
    {
        DirectedGraph<InfluenceUnit> result = new();
        foreach (DirectedEdge<PhysicsProcess> edge in physicsInfluence.AsGraph().Edges())
        {
            foreach (Laboratory fromLab in laboratories)
            {
                foreach (Laboratory toLab in laboratories)
                {
                    result.AddEdge(
                        new InfluenceUnit(edge.source, fromLab),
                        new InfluenceUnit(edge.target, toLab));
                }
            }
        }

        return result;
    }

    private static DirectedGraph<InfluenceUnit> AdjacencyOf(
        PhysicsInfluence physicsInfluence,
        LaboratoryAdjacency laboratoryAdjacency)
    {
        DirectedGraph<InfluenceUnit> result = new();
        foreach (DirectedEdge<PhysicsProcess> physicsEdge in physicsInfluence.AsGraph().Edges())
        {
            foreach (DirectedEdge<Laboratory> adjacencyEdge in laboratoryAdjacency.AsGraph().Edges())
            {
                result.AddEdge(
                    new InfluenceUnit(physicsEdge.source, adjacencyEdge.source),
                    new InfluenceUnit(physicsEdge.target, adjacencyEdge.target));
            }
        }

        return result;
    }

    internal sealed class Builder
    {
        private PhysicsInfluence? physicsInfluence;
        private InfrastructureInfluence infrastructureInfluence = InfrastructureInfluence.CreateBuilder().Build();
        private ISet<Laboratory>? laboratories;
        private LaboratoryAdjacency? laboratoryAdjacency;

        internal Builder WithPhysics(PhysicsInfluence value)
        {
            physicsInfluence = value;
            return this;
        }

        internal Builder WithInfrastructure(InfrastructureInfluence value)
        {
            infrastructureInfluence = value;
            return this;
        }

        internal Builder WithLaboratories(ISet<Laboratory> value)
        {
            laboratories = value;
            return this;
        }

        internal Builder WithLaboratoryAdjacency(LaboratoryAdjacency value)
        {
            laboratoryAdjacency = value;
            return this;
        }

        internal InfluenceMap Build()
        {
            PhysicsInfluence physics = physicsInfluence ?? throw new NullReferenceException();
            return laboratoryAdjacency is not null
                ? Of(physics, infrastructureInfluence, laboratoryAdjacency)
                : Of(physics, infrastructureInfluence, laboratories ?? throw new NullReferenceException());
        }
    }
}
