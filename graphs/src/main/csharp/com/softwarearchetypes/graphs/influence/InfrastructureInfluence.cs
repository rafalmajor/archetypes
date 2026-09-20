namespace com.softwarearchetypes.graphs.influence;

internal sealed class InfrastructureInfluence
{
    private readonly DirectedGraph<InfluenceUnit> graph;

    private InfrastructureInfluence(DirectedGraph<InfluenceUnit> graph)
    {
        this.graph = graph;
    }

    internal static Builder builder() => new();

    internal DirectedGraph<InfluenceUnit> asGraph() => graph;

    internal sealed class Builder
    {
        private readonly DirectedGraph<InfluenceUnit> graph = new();

        internal Builder addConstraint(
            PhysicsProcess fromProcess,
            Laboratory fromLab,
            PhysicsProcess toProcess,
            Laboratory toLab)
        {
            graph.addEdge(
                new InfluenceUnit(fromProcess, fromLab),
                new InfluenceUnit(toProcess, toLab));
            return this;
        }

        internal InfrastructureInfluence build() => new(graph);
    }
}
