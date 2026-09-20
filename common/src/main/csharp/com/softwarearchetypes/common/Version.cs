using System;

namespace com.softwarearchetypes.common;

public sealed class Version : IEquatable<Version>
{
    private const long INITIAL_VALUE = 0L;
    private readonly long storedValue;

    public Version(long value)
    {
        storedValue = value;
    }

    public static Version initial() => new(INITIAL_VALUE);

    public static Version of(long value) => new(value);

    public long value() => storedValue;

    public bool Equals(Version? other) => other is not null && storedValue == other.storedValue;

    public override bool Equals(object? obj) => obj is Version other && Equals(other);

    public override int GetHashCode() => storedValue.GetHashCode();

    public override string ToString() => $"Version[value={storedValue}]";
}
