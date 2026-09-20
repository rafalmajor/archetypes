using System.Collections.Generic;

namespace SoftwareArchetypes.Graphs.Cycles.Math;

public class Graph<T, P>
{
    private readonly Dictionary<Node<T>, List<Edge<T, P>>> adjacencyMatrix = [];

    public Graph<T, P> AddEdge(Edge<T, P> edge)
    {
        if (!adjacencyMatrix.TryGetValue(edge.From(), out List<Edge<T, P>>? edges))
        {
            edges = [];
            adjacencyMatrix[edge.From()] = edges;
        }

        edges.Add(edge);
        adjacencyMatrix.TryAdd(edge.To(), []);
        return this;
    }

    public Path<T, P>? FindFirstCycle()
    {
        HashSet<Node<T>> visited = [];
        HashSet<Node<T>> inStack = [];
        foreach (Node<T> node in adjacencyMatrix.Keys)
        {
            if (!visited.Contains(node))
            {
                Path<T, P>? cycle = FindCycleDFS(node, visited, inStack, []);
                if (cycle is not null)
                {
                    return cycle;
                }
            }
        }

        return null;
    }

    private Path<T, P>? FindCycleDFS(
        Node<T> current,
        ISet<Node<T>> visited,
        ISet<Node<T>> inStack,
        List<Edge<T, P>> path)
    {
        visited.Add(current);
        inStack.Add(current);
        if (adjacencyMatrix.TryGetValue(current, out List<Edge<T, P>>? edges))
        {
            foreach (Edge<T, P> edge in edges)
            {
                Node<T> neighbor = edge.To();
                if (inStack.Contains(neighbor))
                {
                    List<Edge<T, P>> cycle = [];
                    bool foundStart = false;
                    foreach (Edge<T, P> pathEdge in path)
                    {
                        if (pathEdge.From().Equals(neighbor) || foundStart)
                        {
                            foundStart = true;
                            cycle.Add(pathEdge);
                        }
                    }

                    cycle.Add(edge);
                    return new Path<T, P>(cycle);
                }

                if (!visited.Contains(neighbor))
                {
                    path.Add(edge);
                    Path<T, P>? cycle = FindCycleDFS(neighbor, visited, inStack, path);
                    if (cycle is not null)
                    {
                        return cycle;
                    }

                    path.RemoveAt(path.Count - 1);
                }
            }
        }

        inStack.Remove(current);
        return null;
    }

    public bool HasEdge(Node<T> from, Node<T> to) =>
        adjacencyMatrix.TryGetValue(from, out List<Edge<T, P>>? edges) &&
        edges.Exists(edge => edge.To().Equals(to));

    public Graph<T, P> Intersection<P2>(Graph<T, P2> other)
    {
        Graph<T, P> result = new();
        foreach (List<Edge<T, P>> edges in adjacencyMatrix.Values)
        {
            foreach (Edge<T, P> edge in edges)
            {
                if (other.HasEdge(edge.From(), edge.To()))
                {
                    result.AddEdge(edge);
                }
            }
        }

        return result;
    }

    public void RemoveEdge(Edge<T, P> edge)
    {
        if (!adjacencyMatrix.TryGetValue(edge.From(), out List<Edge<T, P>>? edges))
        {
            return;
        }

        for (int index = 0; index < edges.Count; index++)
        {
            if (edges[index].To().Equals(edge.To()))
            {
                edges.RemoveAt(index);
            }
        }
    }
}
