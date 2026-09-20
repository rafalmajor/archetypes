namespace com.softwarearchetypes.graphs.cycles.math;

public sealed record class Edge<T, P>(Node<T> fromValue, Node<T> toValue, P? propertyValue)
{
    internal Edge(Node<T> from, Node<T> to)
        : this(from, to, default)
    {
    }

    public Node<T> from() => fromValue;

    public Node<T> to() => toValue;

    public P? property() => propertyValue;

    public override string ToString() => $"{fromValue} -> {toValue}";
}
