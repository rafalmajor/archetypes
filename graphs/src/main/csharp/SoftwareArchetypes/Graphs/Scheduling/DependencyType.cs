using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SoftwareArchetypes.Graphs.Scheduling;

internal sealed class DependencyType : IEquatable<DependencyType>
{
    private readonly string nameValue;
    private readonly IReadOnlyDictionary<string, object> featuresValue;

    internal DependencyType(string name, IDictionary<string, object> features)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Dependency type name cannot be null or blank");
        }

        nameValue = name;
        featuresValue = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(features));
    }

    internal DependencyType(string name)
        : this(name, new Dictionary<string, object>())
    {
    }

    internal string Name() => nameValue;

    internal IReadOnlyDictionary<string, object> Features() => featuresValue;

    internal static DependencyType FinishToStart(string description) =>
        new("FINISH_TO_START", new Dictionary<string, object> { ["description"] = description });

    internal static DependencyType RequiredResource(string resourceName) =>
        new("REQUIRED_RESOURCE", new Dictionary<string, object> { ["resource"] = resourceName });

    internal static DependencyType DataFlow(string dataType) =>
        new("DATA_FLOW", new Dictionary<string, object> { ["dataType"] = dataType });

    internal static DependencyType Custom(string name, IDictionary<string, object> features) => new(name, features);

    public bool Equals(DependencyType? other)
    {
        if (other is null || nameValue != other.nameValue || featuresValue.Count != other.featuresValue.Count)
        {
            return false;
        }

        foreach ((string key, object value) in featuresValue)
        {
            if (!other.featuresValue.TryGetValue(key, out object? otherValue) || !Equals(value, otherValue))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is DependencyType other && Equals(other);

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(nameValue);
        foreach ((string key, object value) in featuresValue)
        {
            hash.Add(key);
            hash.Add(value);
        }

        return hash.ToHashCode();
    }
}
