using System.Collections.Generic;

namespace com.softwarearchetypes.graphs.influence;

internal sealed class InfluanceAnalyzer
{
    private readonly InfluenceMap influenceMap;

    internal InfluanceAnalyzer(InfluenceMap influenceMap)
    {
        this.influenceMap = influenceMap;
    }

    internal int countConflicts(Reservation newReservation, ISet<Reservation> existingReservations)
    {
        int conflicts = 0;
        foreach (Reservation existing in existingReservations)
        {
            if (influenceMap.influences(newReservation, existing))
            {
                conflicts++;
            }
        }

        return conflicts;
    }

    internal ISet<InfluenceZone> analyzeInfluenceZones(ISet<Reservation> reservations)
    {
        Dictionary<Reservation, HashSet<Reservation>> graph = buildInfluenceGraph(reservations);
        HashSet<Reservation> visited = [];
        HashSet<InfluenceZone> zones = [];
        foreach (Reservation reservation in reservations)
        {
            if (visited.Contains(reservation))
            {
                continue;
            }

            HashSet<Reservation> component = connectedSet(reservation, graph);
            visited.UnionWith(component);
            zones.Add(new InfluenceZone(component));
        }

        return zones;
    }

    internal InfluenceZone findInfluenceZone(Reservation reservation, ISet<Reservation> allReservations) =>
        new(connectedSet(reservation, buildInfluenceGraph(allReservations)));

    internal BridgingReservations identifyCriticalReservations(ISet<Reservation> reservations)
    {
        Dictionary<Reservation, HashSet<Reservation>> graph = buildInfluenceGraph(reservations);
        Dictionary<Reservation, int> discovery = [];
        Dictionary<Reservation, int> low = [];
        HashSet<Reservation> cutpoints = [];
        int time = 0;
        foreach (Reservation reservation in reservations)
        {
            if (!discovery.ContainsKey(reservation))
            {
                findCutpoints(reservation, null, graph, discovery, low, cutpoints, ref time);
            }
        }

        return new BridgingReservations(cutpoints);
    }

    private Dictionary<Reservation, HashSet<Reservation>> buildInfluenceGraph(ISet<Reservation> reservations)
    {
        Dictionary<Reservation, HashSet<Reservation>> graph = [];
        foreach (Reservation reservation in reservations)
        {
            graph.Add(reservation, []);
        }

        foreach (Reservation from in reservations)
        {
            foreach (Reservation to in reservations)
            {
                if (!from.Equals(to) && influenceMap.influences(from, to))
                {
                    graph[from].Add(to);
                    graph[to].Add(from);
                }
            }
        }

        return graph;
    }

    private static HashSet<Reservation> connectedSet(
        Reservation reservation,
        Dictionary<Reservation, HashSet<Reservation>> graph)
    {
        HashSet<Reservation> connected = [];
        Stack<Reservation> pending = new();
        pending.Push(reservation);
        while (pending.Count > 0)
        {
            Reservation current = pending.Pop();
            if (!connected.Add(current))
            {
                continue;
            }

            foreach (Reservation neighbor in graph[current])
            {
                pending.Push(neighbor);
            }
        }

        return connected;
    }

    private static void findCutpoints(
        Reservation current,
        Reservation? parent,
        Dictionary<Reservation, HashSet<Reservation>> graph,
        Dictionary<Reservation, int> discovery,
        Dictionary<Reservation, int> low,
        ISet<Reservation> cutpoints,
        ref int time)
    {
        discovery[current] = ++time;
        low[current] = discovery[current];
        int children = 0;
        foreach (Reservation neighbor in graph[current])
        {
            if (!discovery.ContainsKey(neighbor))
            {
                children++;
                findCutpoints(neighbor, current, graph, discovery, low, cutpoints, ref time);
                low[current] = System.Math.Min(low[current], low[neighbor]);
                if (parent is null && children > 1 ||
                    parent is not null && low[neighbor] >= discovery[current])
                {
                    cutpoints.Add(current);
                }
            }
            else if (!neighbor.Equals(parent))
            {
                low[current] = System.Math.Min(low[current], discovery[neighbor]);
            }
        }
    }
}
