using System;
using System.Collections.Generic;
using System.Linq;

namespace com.softwarearchetypes.graphs.userjourney;

internal sealed class CustomerPath : IEquatable<CustomerPath>
{
    private readonly IReadOnlyList<Condition> conditionsValue;

    internal CustomerPath(IReadOnlyList<Condition> conditions)
    {
        conditionsValue = conditions;
    }

    internal IReadOnlyList<Condition> conditions() => conditionsValue;

    internal static CustomerPath of(IEnumerable<Condition> conditions) => new(conditions.ToList().AsReadOnly());

    internal int length() => conditionsValue.Count;

    internal bool isEmpty() => conditionsValue.Count == 0;

    internal double weight(Func<Condition, double> weightFunction)
    {
        double sum = 0;
        foreach (Condition condition in conditionsValue)
        {
            sum += weightFunction(condition);
        }

        return sum;
    }

    public bool Equals(CustomerPath? other) =>
        other is not null && conditionsValue.SequenceEqual(other.conditionsValue);

    public override bool Equals(object? obj) => obj is CustomerPath other && Equals(other);

    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (Condition condition in conditionsValue)
        {
            hash.Add(condition);
        }

        return hash.ToHashCode();
    }
}
