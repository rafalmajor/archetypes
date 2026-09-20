namespace com.softwarearchetypes.graphs.cycles;

internal sealed class Slot
{
    private readonly SlotId slotId;
    private OwnerId owner;

    internal Slot(SlotId slotId, OwnerId owner)
    {
        this.slotId = slotId;
        this.owner = owner;
    }

    internal static Slot create(SlotId slotId, OwnerId owner) => new(slotId, owner);

    internal void release() => owner = OwnerId.empty();

    internal void assignTo(OwnerId newOwner) => owner = newOwner;

    internal OwnerId getOwner() => owner;

    internal SlotId id() => slotId;
}
