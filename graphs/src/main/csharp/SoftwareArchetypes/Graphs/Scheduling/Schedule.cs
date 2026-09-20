using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftwareArchetypes.Graphs.Scheduling;

internal sealed class Schedule : IEquatable<Schedule>
{
    private readonly IReadOnlyList<ProcessStep> stepsValue;

    internal Schedule(IEnumerable<ProcessStep> steps)
    {
        stepsValue = steps.ToList().AsReadOnly();
    }

    internal IReadOnlyList<ProcessStep> Steps() => stepsValue;

    internal ProcessStep? First() => stepsValue.Count == 0 ? null : stepsValue[0];

    internal ProcessStep? Last() => stepsValue.Count == 0 ? null : stepsValue[^1];

    internal int Size() => stepsValue.Count;

    internal bool IsEmpty() => stepsValue.Count == 0;

    public bool Equals(Schedule? other) => other is not null && stepsValue.SequenceEqual(other.stepsValue);

    public override bool Equals(object? obj) => obj is Schedule other && Equals(other);

    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (ProcessStep step in stepsValue)
        {
            hash.Add(step);
        }

        return hash.ToHashCode();
    }
}
