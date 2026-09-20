using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace com.softwarearchetypes.graphs.influence;

internal sealed class PhysicsInfluence
{
    private static readonly IDictionary<string, int> EmptyRequirements =
        new ReadOnlyDictionary<string, int>(new Dictionary<string, int>());

    private readonly DirectedGraph<PhysicsProcess> graph;
    private readonly Dictionary<EdgeKey, IDictionary<string, int>> edgeFeatureRequirements;

    private PhysicsInfluence(
        DirectedGraph<PhysicsProcess> graph,
        Dictionary<EdgeKey, IDictionary<string, int>> edgeFeatureRequirements)
    {
        this.graph = graph;
        this.edgeFeatureRequirements = edgeFeatureRequirements;
    }

    internal static Builder builder() => new();

    internal DirectedGraph<PhysicsProcess> asGraph() => graph;

    internal IDictionary<string, int> getFeatureRequirements(PhysicsProcess from, PhysicsProcess to) =>
        edgeFeatureRequirements.GetValueOrDefault(new EdgeKey(from, to), EmptyRequirements);

    internal sealed class Builder
    {
        private readonly DirectedGraph<PhysicsProcess> graph = new();
        private readonly Dictionary<EdgeKey, IDictionary<string, int>> edgeFeatureRequirements = [];

        internal Builder addInfluence(PhysicsProcess from, PhysicsProcess to)
        {
            graph.addEdge(from, to);
            return this;
        }

        internal Builder addInfluence(
            PhysicsProcess from,
            PhysicsProcess to,
            IDictionary<string, int> featureRequirements)
        {
            graph.addEdge(from, to);
            edgeFeatureRequirements[new EdgeKey(from, to)] = new Dictionary<string, int>(featureRequirements);
            return this;
        }

        internal PhysicsInfluence build() => new(graph, edgeFeatureRequirements);
    }

    private sealed record class EdgeKey(object from, object to);
}

internal sealed record class DirectedEdge<T>(T source, T target);

internal sealed class DirectedGraph<T>
    where T : notnull
{
    private readonly HashSet<T> verticesValue = [];
    private readonly List<DirectedEdge<T>> edgesValue = [];
    private readonly HashSet<DirectedEdge<T>> edgeSet = [];

    internal IReadOnlySet<T> vertices() => verticesValue;

    internal IReadOnlyList<DirectedEdge<T>> edges() => edgesValue;

    internal void addVertex(T vertex) => verticesValue.Add(vertex);

    internal void addEdge(T from, T to)
    {
        addVertex(from);
        addVertex(to);
        DirectedEdge<T> edge = new(from, to);
        if (edgeSet.Add(edge))
        {
            edgesValue.Add(edge);
        }
    }

    internal bool containsEdge(T from, T to) => edgeSet.Contains(new DirectedEdge<T>(from, to));
}