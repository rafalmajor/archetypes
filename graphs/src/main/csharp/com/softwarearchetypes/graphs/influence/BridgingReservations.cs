using System;
using System.Collections.Generic;

namespace com.softwarearchetypes.graphs.influence;

internal sealed class BridgingReservations : IEquatable<BridgingReservations>
{
    private readonly IReadOnlySet<Reservation> reservationsValue;

    internal BridgingReservations(IEnumerable<Reservation> reservations)
    {
        reservationsValue = new HashSet<Reservation>(reservations);
    }

    internal IReadOnlySet<Reservation> reservations() => reservationsValue;

    internal bool isBridging(Reservation reservation) => reservationsValue.Contains(reservation);

    internal int count() => reservationsValue.Count;

    internal bool isEmpty() => reservationsValue.Count == 0;

    public bool Equals(BridgingReservations? other) =>
        other is not null && reservationsValue.SetEquals(other.reservationsValue);

    public override bool Equals(object? obj) => obj is BridgingReservations other && Equals(other);

    public override int GetHashCode()
    {
        int hash = 0;
        foreach (Reservation reservation in reservationsValue)
        {
            hash ^= reservation.GetHashCode();
        }

        return hash;
    }
}