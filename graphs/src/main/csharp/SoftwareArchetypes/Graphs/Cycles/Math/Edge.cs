namespace SoftwareArchetypes.Graphs.Cycles.Math;

public sealed record class Edge<T, P>(Node<T> fromValue, Node<T> toValue, P? propertyValue)
{
    internal Edge(Node<T> from, Node<T> to)
        : this(from, to, default)
    {
    }

    public Node<T> From() => fromValue;

    public Node<T> To() => toValue;

    public P? Property() => propertyValue;

    public override string ToString() => $"{fromValue} -> {toValue}";
}
