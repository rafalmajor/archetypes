using System.Collections.Generic;
using System.Linq;

namespace SoftwareArchetypes.Graphs.Cycles;

internal sealed record class BatchReservationResult(
    BatchReservationResult.Status statusValue,
    IReadOnlyList<ReservationChangeRequest> executedRequestsValue)
{
    internal enum Status
    {
        SUCCESS,
        FAILURE
    }

    internal Status GetStatus() => statusValue;

    internal IReadOnlyList<ReservationChangeRequest> ExecutedRequests() => executedRequestsValue;

    internal static BatchReservationResult Success(ISet<ReservationChangeRequest> executed) =>
        new(Status.SUCCESS, executed.ToList());

    internal static BatchReservationResult None() => new(Status.FAILURE, []);
}
