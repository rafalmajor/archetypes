namespace com.softwarearchetypes.graphs.cycles.math;

public sealed record class Node<T>(T? propertyValue)
{
    public T? property() => propertyValue;

    public override string ToString() => propertyValue?.ToString() ?? "null";
}