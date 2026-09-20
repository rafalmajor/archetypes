namespace com.softwarearchetypes.graphs.cycles;

internal sealed record class ReservationChangeRequest(
    SlotId fromSlotValue,
    SlotId toSlotValue,
    OwnerId userIdValue)
{
    internal SlotId fromSlot() => fromSlotValue;

    internal SlotId toSlot() => toSlotValue;

    internal OwnerId userId() => userIdValue;
}