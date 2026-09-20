using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftwareArchetypes.Graphs.Cycles.Math;

public sealed class Path<T, P> : IEquatable<Path<T, P>>
{
    private readonly IReadOnlyList<Edge<T, P>> pathEdges;

    public Path(IReadOnlyList<Edge<T, P>> edges)
    {
        pathEdges = edges;
    }

    public IReadOnlyList<Edge<T, P>> Edges() => pathEdges;

    public bool Equals(Path<T, P>? other) =>
        other is not null && pathEdges.SequenceEqual(other.pathEdges);

    public override bool Equals(object? obj) => obj is Path<T, P> other && Equals(other);

    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (Edge<T, P> edge in pathEdges)
        {
            hash.Add(edge);
        }

        return hash.ToHashCode();
    }

    public override string ToString() => pathEdges.Count == 0
        ? "-"
        : $"{pathEdges[0].From()} -> {string.Join(" -> ", pathEdges.Select(edge => edge.To()))}";
}
