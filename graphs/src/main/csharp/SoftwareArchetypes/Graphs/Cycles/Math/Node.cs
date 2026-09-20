namespace SoftwareArchetypes.Graphs.Cycles.Math;

public sealed record class Node<T>(T? propertyValue)
{
    public T? Property() => propertyValue;

    public override string ToString() => propertyValue?.ToString() ?? "null";
}
