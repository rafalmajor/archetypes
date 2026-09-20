namespace SoftwareArchetypes.Graphs.Cycles;

internal sealed record class ReservationChangeRequest(
    SlotId fromSlotValue,
    SlotId toSlotValue,
    OwnerId userIdValue)
{
    internal SlotId FromSlot() => fromSlotValue;

    internal SlotId ToSlot() => toSlotValue;

    internal OwnerId UserId() => userIdValue;
}
