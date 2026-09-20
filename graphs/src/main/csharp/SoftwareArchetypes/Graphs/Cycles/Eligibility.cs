using SoftwareArchetypes.Graphs.Cycles.Math;

namespace SoftwareArchetypes.Graphs.Cycles;

internal sealed class Eligibility
{
    private readonly Graph<OwnerId, object> graph = new();

    internal void MarkTransferEligible(OwnerId from, OwnerId to) =>
        graph.AddEdge(new Edge<OwnerId, object>(new Node<OwnerId>(from), new Node<OwnerId>(to), null));

    internal void MarkTransferIneligible(OwnerId from, OwnerId to) =>
        graph.RemoveEdge(new Edge<OwnerId, object>(new Node<OwnerId>(from), new Node<OwnerId>(to), null));

    internal bool IsTransferEligible(OwnerId from, OwnerId to) =>
        graph.HasEdge(new Node<OwnerId>(from), new Node<OwnerId>(to));

    internal Graph<OwnerId, object> AsGraph() => graph;
}
