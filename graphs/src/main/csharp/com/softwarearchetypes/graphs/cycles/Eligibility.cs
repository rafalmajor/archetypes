using com.softwarearchetypes.graphs.cycles.math;

namespace com.softwarearchetypes.graphs.cycles;

internal sealed class Eligibility
{
    private readonly Graph<OwnerId, object> graph = new();

    internal void markTransferEligible(OwnerId from, OwnerId to) =>
        graph.addEdge(new Edge<OwnerId, object>(new Node<OwnerId>(from), new Node<OwnerId>(to), null));

    internal void markTransferIneligible(OwnerId from, OwnerId to) =>
        graph.removeEdge(new Edge<OwnerId, object>(new Node<OwnerId>(from), new Node<OwnerId>(to), null));

    internal bool isTransferEligible(OwnerId from, OwnerId to) =>
        graph.hasEdge(new Node<OwnerId>(from), new Node<OwnerId>(to));

    internal Graph<OwnerId, object> asGraph() => graph;
}
