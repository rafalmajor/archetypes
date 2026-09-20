using System;
using System.Collections.Generic;

namespace SoftwareArchetypes.Graphs.Influence;

internal sealed class BridgingReservations : IEquatable<BridgingReservations>
{
    private readonly IReadOnlySet<Reservation> reservationsValue;

    internal BridgingReservations(IEnumerable<Reservation> reservations)
    {
        reservationsValue = new HashSet<Reservation>(reservations);
    }

    internal IReadOnlySet<Reservation> Reservations() => reservationsValue;

    internal bool IsBridging(Reservation reservation) => reservationsValue.Contains(reservation);

    internal int Count() => reservationsValue.Count;

    internal bool IsEmpty() => reservationsValue.Count == 0;

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
