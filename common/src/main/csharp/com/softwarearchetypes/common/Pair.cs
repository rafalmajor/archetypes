using System;

namespace com.softwarearchetypes.common;

public sealed class Pair<T> : IEquatable<Pair<T>>
{
    private readonly T? firstValue;
    private readonly T? secondValue;

    public Pair(T? first, T? second)
    {
        firstValue = first;
        secondValue = second;
    }

    public static Pair<T> of(T? first, T? second) => new(first, second);

    public T? first() => firstValue;

    public T? second() => secondValue;

    public bool Equals(Pair<T>? other) =>
        other is not null &&
        Equals(firstValue, other.firstValue) &&
        Equals(secondValue, other.secondValue);

    public override bool Equals(object? obj) => obj is Pair<T> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(firstValue, secondValue);

    public override string ToString() => $"Pair[first={firstValue}, second={secondValue}]";
}
