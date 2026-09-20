using System;
using System.Collections.Generic;
using SoftwareArchetypes.Graphs.Cycles.Math;

namespace SoftwareArchetypes.Graphs.Cycles;

internal sealed class BatchReservationUseCase
{
    private readonly SlotRepository slotRepository;

    internal BatchReservationUseCase(SlotRepository slotRepository)
    {
        this.slotRepository = slotRepository;
    }

    internal BatchReservationResult Execute(IReadOnlyList<ReservationChangeRequest> requests)
    {
        Graph<SlotId, ReservationChangeRequest> graph = BuildGraph(requests);
        ISet<ReservationChangeRequest> dependentRequests = FindDependentRequests(graph, requests);
        return Execute(dependentRequests);
    }

    internal BatchReservationResult Execute(
        IReadOnlyList<ReservationChangeRequest> requests,
        Eligibility eligibility)
    {
        Graph<OwnerId, ReservationChangeRequest> intersection =
            BuildOwnerGraph(requests).Intersection(eligibility.AsGraph());
        return Execute(FindDependentRequestsInOwnerGraph(intersection));
    }

    private BatchReservationResult Execute(ISet<ReservationChangeRequest> dependentRequests)
    {
        if (dependentRequests.Count == 0)
        {
            return BatchReservationResult.None();
        }

        IDictionary<SlotId, Slot> slots = LoadAllSlots(dependentRequests);
        foreach (ReservationChangeRequest request in dependentRequests)
        {
            slots[request.FromSlot()].Release();
        }

        foreach (ReservationChangeRequest request in dependentRequests)
        {
            slots[request.ToSlot()].AssignTo(request.UserId());
        }

        slotRepository.SaveAll(slots.Values);
        return BatchReservationResult.Success(dependentRequests);
    }

    private static Graph<SlotId, ReservationChangeRequest> BuildGraph(
        IReadOnlyList<ReservationChangeRequest> requests)
    {
        Graph<SlotId, ReservationChangeRequest> graph = new();
        foreach (ReservationChangeRequest request in requests)
        {
            graph.AddEdge(new Edge<SlotId, ReservationChangeRequest>(
                new Node<SlotId>(request.FromSlot()),
                new Node<SlotId>(request.ToSlot()),
                request));
        }

        return graph;
    }

    private IDictionary<SlotId, Slot> LoadAllSlots(ISet<ReservationChangeRequest> dependentRequests)
    {
        HashSet<SlotId> allSlotIds = [];
        foreach (ReservationChangeRequest request in dependentRequests)
        {
            allSlotIds.Add(request.FromSlot());
            allSlotIds.Add(request.ToSlot());
        }

        return slotRepository.FindAll(allSlotIds);
    }

    private Graph<OwnerId, ReservationChangeRequest> BuildOwnerGraph(
        IReadOnlyList<ReservationChangeRequest> requests)
    {
        HashSet<SlotId> allSlotIds = [];
        foreach (ReservationChangeRequest request in requests)
        {
            allSlotIds.Add(request.FromSlot());
            allSlotIds.Add(request.ToSlot());
        }

        IDictionary<SlotId, Slot> slots = slotRepository.FindAll(allSlotIds);
        Graph<OwnerId, ReservationChangeRequest> graph = new();
        foreach (ReservationChangeRequest request in requests)
        {
            if (slots.TryGetValue(request.FromSlot(), out Slot? fromSlot) &&
                slots.TryGetValue(request.ToSlot(), out Slot? toSlot))
            {
                graph.AddEdge(new Edge<OwnerId, ReservationChangeRequest>(
                    new Node<OwnerId>(fromSlot.GetOwner()),
                    new Node<OwnerId>(toSlot.GetOwner()),
                    request));
            }
        }

        return graph;
    }

    private static ISet<ReservationChangeRequest> FindDependentRequestsInOwnerGraph(
        Graph<OwnerId, ReservationChangeRequest> graph) => CycleRequests(graph.FindFirstCycle());

    private static ISet<ReservationChangeRequest> FindDependentRequests(
        Graph<SlotId, ReservationChangeRequest> graph,
        IReadOnlyList<ReservationChangeRequest> requests) => CycleRequests(graph.FindFirstCycle());

    private static ISet<ReservationChangeRequest> CycleRequests<T>(
        Path<T, ReservationChangeRequest>? path)
    {
        HashSet<ReservationChangeRequest> result = [];
        if (path is null)
        {
            return result;
        }

        foreach (Edge<T, ReservationChangeRequest> edge in path.Edges())
        {
            ReservationChangeRequest request = edge.Property() ?? throw new NullReferenceException();
            result.Add(request);
        }

        return result;
    }
}
