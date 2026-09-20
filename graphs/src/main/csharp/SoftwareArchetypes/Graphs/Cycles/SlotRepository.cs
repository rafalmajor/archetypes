using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SoftwareArchetypes.Graphs.Cycles;

internal interface SlotRepository
{
    Slot? FindById(SlotId slotId);

    void Save(Slot slot);

    void SaveAll(IEnumerable<Slot> values);

    IDictionary<SlotId, Slot> FindAll(ISet<SlotId> allSlotIds);
}

internal sealed class InMemorySlotRepository : SlotRepository
{
    private readonly ConcurrentDictionary<SlotId, Slot> slots = new();

    public Slot? FindById(SlotId slotId) => slots.GetValueOrDefault(slotId);

    public void Save(Slot slot) => slots[slot.Id()] = slot;

    public IDictionary<SlotId, Slot> FindAll(ISet<SlotId> allSlotIds)
    {
        Dictionary<SlotId, Slot> result = [];
        foreach (SlotId slotId in allSlotIds)
        {
            Slot original = FindById(slotId) ?? throw new NullReferenceException();
            result.Add(slotId, Slot.Create(original.Id(), original.GetOwner()));
        }

        return result;
    }

    public void SaveAll(IEnumerable<Slot> values)
    {
        foreach (Slot slot in values)
        {
            Save(slot);
        }
    }
}
