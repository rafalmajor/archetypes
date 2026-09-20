using System.Collections.Generic;
using System.Linq;

namespace com.softwarearchetypes.graphs.cycles;

internal sealed record class BatchReservationResult(
    BatchReservationResult.Status statusValue,
    IReadOnlyList<ReservationChangeRequest> executedRequestsValue)
{
    internal enum Status
    {
        SUCCESS,
        FAILURE
    }

    internal Status status() => statusValue;

    internal IReadOnlyList<ReservationChangeRequest> executedRequests() => executedRequestsValue;

    internal static BatchReservationResult success(ISet<ReservationChangeRequest> executed) =>
        new(Status.SUCCESS, executed.ToList());

    internal static BatchReservationResult none() => new(Status.FAILURE, []);
}