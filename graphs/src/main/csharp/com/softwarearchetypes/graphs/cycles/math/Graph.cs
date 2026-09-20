using System.Collections.Generic;

namespace com.softwarearchetypes.graphs.cycles.math;

public class Graph<T, P>
{
    private readonly Dictionary<Node<T>, List<Edge<T, P>>> adjacencyMatrix = [];

    public Graph<T, P> addEdge(Edge<T, P> edge)
    {
        if (!adjacencyMatrix.TryGetValue(edge.from(), out List<Edge<T, P>>? edges))
        {
            edges = [];
            adjacencyMatrix[edge.from()] = edges;
        }

        edges.Add(edge);
        adjacencyMatrix.TryAdd(edge.to(), []);
        return this;
    }

    public Path<T, P>? findFirstCycle()
    {
        HashSet<Node<T>> visited = [];
        HashSet<Node<T>> inStack = [];
        foreach (Node<T> node in adjacencyMatrix.Keys)
        {
            if (!visited.Contains(node))
            {
                Path<T, P>? cycle = findCycleDFS(node, visited, inStack, []);
                if (cycle is not null)
                {
                    return cycle;
                }
            }
        }

        return null;
    }

    private Path<T, P>? findCycleDFS(
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
                Node<T> neighbor = edge.to();
                if (inStack.Contains(neighbor))
                {
                    List<Edge<T, P>> cycle = [];
                    bool foundStart = false;
                    foreach (Edge<T, P> pathEdge in path)
                    {
                        if (pathEdge.from().Equals(neighbor) || foundStart)
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
                    Path<T, P>? cycle = findCycleDFS(neighbor, visited, inStack, path);
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

    public bool hasEdge(Node<T> from, Node<T> to) =>
        adjacencyMatrix.TryGetValue(from, out List<Edge<T, P>>? edges) &&
        edges.Exists(edge => edge.to().Equals(to));

    public Graph<T, P> intersection<P2>(Graph<T, P2> other)
    {
        Graph<T, P> result = new();
        foreach (List<Edge<T, P>> edges in adjacencyMatrix.Values)
        {
            foreach (Edge<T, P> edge in edges)
            {
                if (other.hasEdge(edge.from(), edge.to()))
                {
                    result.addEdge(edge);
                }
            }
        }

        return result;
    }

    public void removeEdge(Edge<T, P> edge)
    {
        if (!adjacencyMatrix.TryGetValue(edge.from(), out List<Edge<T, P>>? edges))
        {
            return;
        }

        for (int index = 0; index < edges.Count; index++)
        {
            if (edges[index].to().Equals(edge.to()))
            {
                edges.RemoveAt(index);
            }
        }
    }
}
