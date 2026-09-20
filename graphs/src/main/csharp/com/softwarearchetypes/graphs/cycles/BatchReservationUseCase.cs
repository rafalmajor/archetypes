using System;
using System.Collections.Generic;
using com.softwarearchetypes.graphs.cycles.math;

namespace com.softwarearchetypes.graphs.cycles;

internal sealed class BatchReservationUseCase
{
    private readonly SlotRepository slotRepository;

    internal BatchReservationUseCase(SlotRepository slotRepository)
    {
        this.slotRepository = slotRepository;
    }

    internal BatchReservationResult execute(IReadOnlyList<ReservationChangeRequest> requests)
    {
        Graph<SlotId, ReservationChangeRequest> graph = buildGraph(requests);
        ISet<ReservationChangeRequest> dependentRequests = findDependentRequests(graph, requests);
        return execute(dependentRequests);
    }

    internal BatchReservationResult execute(
        IReadOnlyList<ReservationChangeRequest> requests,
        Eligibility eligibility)
    {
        Graph<OwnerId, ReservationChangeRequest> intersection =
            buildOwnerGraph(requests).intersection(eligibility.asGraph());
        return execute(findDependentRequestsInOwnerGraph(intersection));
    }

    private BatchReservationResult execute(ISet<ReservationChangeRequest> dependentRequests)
    {
        if (dependentRequests.Count == 0)
        {
            return BatchReservationResult.none();
        }

        IDictionary<SlotId, Slot> slots = loadAllSlots(dependentRequests);
        foreach (ReservationChangeRequest request in dependentRequests)
        {
            slots[request.fromSlot()].release();
        }

        foreach (ReservationChangeRequest request in dependentRequests)
        {
            slots[request.toSlot()].assignTo(request.userId());
        }

        slotRepository.saveAll(slots.Values);
        return BatchReservationResult.success(dependentRequests);
    }

    private static Graph<SlotId, ReservationChangeRequest> buildGraph(
        IReadOnlyList<ReservationChangeRequest> requests)
    {
        Graph<SlotId, ReservationChangeRequest> graph = new();
        foreach (ReservationChangeRequest request in requests)
        {
            graph.addEdge(new Edge<SlotId, ReservationChangeRequest>(
                new Node<SlotId>(request.fromSlot()),
                new Node<SlotId>(request.toSlot()),
                request));
        }

        return graph;
    }

    private IDictionary<SlotId, Slot> loadAllSlots(ISet<ReservationChangeRequest> dependentRequests)
    {
        HashSet<SlotId> allSlotIds = [];
        foreach (ReservationChangeRequest request in dependentRequests)
        {
            allSlotIds.Add(request.fromSlot());
            allSlotIds.Add(request.toSlot());
        }

        return slotRepository.findAll(allSlotIds);
    }

    private Graph<OwnerId, ReservationChangeRequest> buildOwnerGraph(
        IReadOnlyList<ReservationChangeRequest> requests)
    {
        HashSet<SlotId> allSlotIds = [];
        foreach (ReservationChangeRequest request in requests)
        {
            allSlotIds.Add(request.fromSlot());
            allSlotIds.Add(request.toSlot());
        }

        IDictionary<SlotId, Slot> slots = slotRepository.findAll(allSlotIds);
        Graph<OwnerId, ReservationChangeRequest> graph = new();
        foreach (ReservationChangeRequest request in requests)
        {
            if (slots.TryGetValue(request.fromSlot(), out Slot? fromSlot) &&
                slots.TryGetValue(request.toSlot(), out Slot? toSlot))
            {
                graph.addEdge(new Edge<OwnerId, ReservationChangeRequest>(
                    new Node<OwnerId>(fromSlot.getOwner()),
                    new Node<OwnerId>(toSlot.getOwner()),
                    request));
            }
        }

        return graph;
    }

    private static ISet<ReservationChangeRequest> findDependentRequestsInOwnerGraph(
        Graph<OwnerId, ReservationChangeRequest> graph) => cycleRequests(graph.findFirstCycle());

    private static ISet<ReservationChangeRequest> findDependentRequests(
        Graph<SlotId, ReservationChangeRequest> graph,
        IReadOnlyList<ReservationChangeRequest> requests) => cycleRequests(graph.findFirstCycle());

    private static ISet<ReservationChangeRequest> cycleRequests<T>(
        Path<T, ReservationChangeRequest>? path)
    {
        HashSet<ReservationChangeRequest> result = [];
        if (path is null)
        {
            return result;
        }

        foreach (Edge<T, ReservationChangeRequest> edge in path.edges())
        {
            ReservationChangeRequest request = edge.property() ?? throw new NullReferenceException();
            result.Add(request);
        }

        return result;
    }
}
