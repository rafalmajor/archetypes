namespace SoftwareArchetypes.Graphs.Cycles;

internal sealed class Slot
{
    private readonly SlotId slotId;
    private OwnerId owner;

    internal Slot(SlotId slotId, OwnerId owner)
    {
        this.slotId = slotId;
        this.owner = owner;
    }

    internal static Slot Create(SlotId slotId, OwnerId owner) => new(slotId, owner);

    internal void Release() => owner = OwnerId.Empty();

    internal void AssignTo(OwnerId newOwner) => owner = newOwner;

    internal OwnerId GetOwner() => owner;

    internal SlotId Id() => slotId;
}
