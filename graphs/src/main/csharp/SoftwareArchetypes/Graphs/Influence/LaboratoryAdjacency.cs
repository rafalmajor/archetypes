namespace SoftwareArchetypes.Graphs.Influence;

internal sealed class LaboratoryAdjacency
{
    private readonly DirectedGraph<Laboratory> graph;

    private LaboratoryAdjacency(DirectedGraph<Laboratory> graph)
    {
        this.graph = graph;
    }

    internal static Builder CreateBuilder() => new();

    internal DirectedGraph<Laboratory> AsGraph() => graph;

    internal sealed class Builder
    {
        private readonly DirectedGraph<Laboratory> graph = new();

        internal Builder Adjacent(Laboratory from, Laboratory to)
        {
            graph.AddEdge(from, to);
            return this;
        }

        internal LaboratoryAdjacency Build() => new(graph);
    }
}
