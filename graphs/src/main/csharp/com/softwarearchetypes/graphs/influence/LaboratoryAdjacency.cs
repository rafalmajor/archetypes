namespace com.softwarearchetypes.graphs.influence;

internal sealed class LaboratoryAdjacency
{
    private readonly DirectedGraph<Laboratory> graph;

    private LaboratoryAdjacency(DirectedGraph<Laboratory> graph)
    {
        this.graph = graph;
    }

    internal static Builder builder() => new();

    internal DirectedGraph<Laboratory> asGraph() => graph;

    internal sealed class Builder
    {
        private readonly DirectedGraph<Laboratory> graph = new();

        internal Builder adjacent(Laboratory from, Laboratory to)
        {
            graph.addEdge(from, to);
            return this;
        }

        internal LaboratoryAdjacency build() => new(graph);
    }
}
