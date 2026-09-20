using System;
using System.Collections.Generic;

namespace SoftwareArchetypes.Graphs.Influence;

internal sealed class InfluenceZone : IEquatable<InfluenceZone>
{
    private readonly IReadOnlySet<Reservation> reservationsValue;

    internal InfluenceZone(IEnumerable<Reservation> reservations)
    {
        reservationsValue = new HashSet<Reservation>(reservations);
    }

    internal IReadOnlySet<Reservation> Reservations() => reservationsValue;

    internal int CountReservationsToNegotiateWith(Reservation reservation) =>
        reservationsValue.Contains(reservation) ? reservationsValue.Count - 1 : 0;

    internal IReadOnlySet<Reservation> GetReservationsToNegotiateWith(Reservation reservation)
    {
        if (!reservationsValue.Contains(reservation))
        {
            return new HashSet<Reservation>();
        }

        HashSet<Reservation> result = new(reservationsValue);
        result.Remove(reservation);
        return result;
    }

    internal int Size() => reservationsValue.Count;

    public bool Equals(InfluenceZone? other) =>
        other is not null && reservationsValue.SetEquals(other.reservationsValue);

    public override bool Equals(object? obj) => obj is InfluenceZone other && Equals(other);

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
