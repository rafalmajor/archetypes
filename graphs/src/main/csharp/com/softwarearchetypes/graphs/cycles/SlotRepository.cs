using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace com.softwarearchetypes.graphs.cycles;

internal interface SlotRepository
{
    Slot? findById(SlotId slotId);

    void save(Slot slot);

    void saveAll(IEnumerable<Slot> values);

    IDictionary<SlotId, Slot> findAll(ISet<SlotId> allSlotIds);
}

internal sealed class InMemorySlotRepository : SlotRepository
{
    private readonly ConcurrentDictionary<SlotId, Slot> slots = new();

    public Slot? findById(SlotId slotId) => slots.GetValueOrDefault(slotId);

    public void save(Slot slot) => slots[slot.id()] = slot;

    public IDictionary<SlotId, Slot> findAll(ISet<SlotId> allSlotIds)
    {
        Dictionary<SlotId, Slot> result = [];
        foreach (SlotId slotId in allSlotIds)
        {
            Slot original = findById(slotId) ?? throw new NullReferenceException();
            result.Add(slotId, Slot.create(original.id(), original.getOwner()));
        }

        return result;
    }

    public void saveAll(IEnumerable<Slot> values)
    {
        foreach (Slot slot in values)
        {
            save(slot);
        }
    }
}