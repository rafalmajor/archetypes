namespace SoftwareArchetypes.Graphs.Influence;

internal sealed class InfrastructureInfluence
{
    private readonly DirectedGraph<InfluenceUnit> graph;

    private InfrastructureInfluence(DirectedGraph<InfluenceUnit> graph)
    {
        this.graph = graph;
    }

    internal static Builder CreateBuilder() => new();

    internal DirectedGraph<InfluenceUnit> AsGraph() => graph;

    internal sealed class Builder
    {
        private readonly DirectedGraph<InfluenceUnit> graph = new();

        internal Builder AddConstraint(
            PhysicsProcess fromProcess,
            Laboratory fromLab,
            PhysicsProcess toProcess,
            Laboratory toLab)
        {
            graph.AddEdge(
                new InfluenceUnit(fromProcess, fromLab),
                new InfluenceUnit(toProcess, toLab));
            return this;
        }

        internal InfrastructureInfluence Build() => new(graph);
    }
}
