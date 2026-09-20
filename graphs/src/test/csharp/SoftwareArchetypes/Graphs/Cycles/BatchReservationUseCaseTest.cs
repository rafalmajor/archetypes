using System.Collections.Generic;
using Xunit;

namespace SoftwareArchetypes.Graphs.Cycles;

public sealed class BatchReservationUseCaseTest
{
    private readonly SlotRepository slotRepository = new InMemorySlotRepository();

    [Fact]
    public void ExecutesDependentReservationChangesWhenSlotsRemainValid()
    {
        SlotId slotA = SlotId.Of("SlotA");
        SlotId slotB = SlotId.Of("SlotB");
        OwnerId userX = OwnerId.Of("UserX");
        OwnerId userY = OwnerId.Of("UserY");
        ThereIsSlotOwnedBy(slotA, userX);
        ThereIsSlotOwnedBy(slotB, userY);

        BatchReservationResult result = UseCase().Execute([
            new ReservationChangeRequest(slotA, slotB, userX),
            new ReservationChangeRequest(slotB, slotA, userY)
        ]);

        Assert.Equal(BatchReservationResult.Status.SUCCESS, result.GetStatus());
        Assert.Equal(2, result.ExecutedRequests().Count);
        Assert.Equal(userY, FindSlotOwner(slotA));
        Assert.Equal(userX, FindSlotOwner(slotB));
    }

    [Fact]
    public void ExecutesDependentReservationChangesAndSkipsInvalidOnes()
    {
        SlotId slotA = SlotId.Of("SlotA");
        SlotId slotB = SlotId.Of("SlotB");
        SlotId slotD = SlotId.Of("SlotD");
        OwnerId userX = OwnerId.Of("UserX");
        OwnerId userY = OwnerId.Of("UserY");
        OwnerId userZ = OwnerId.Of("UserZ");
        ThereIsSlotOwnedBy(slotA, userX);
        ThereIsSlotOwnedBy(slotB, userY);
        ThereIsFreeSlot(slotD);

        BatchReservationResult result = UseCase().Execute([
            new ReservationChangeRequest(slotB, slotA, userY),
            new ReservationChangeRequest(slotA, slotB, userX),
            new ReservationChangeRequest(slotD, slotA, userZ)
        ]);

        Assert.Equal(BatchReservationResult.Status.SUCCESS, result.GetStatus());
        Assert.Equal(2, result.ExecutedRequests().Count);
        Assert.Equal(userY, FindSlotOwner(slotA));
        Assert.Equal(userX, FindSlotOwner(slotB));
        Assert.Equal(OwnerId.Empty(), FindSlotOwner(slotD));
    }

    [Fact]
    public void ExecutesComplexDependentReservationChangesWithMultipleSlots()
    {
        SlotId slotA = SlotId.Of("SlotA");
        SlotId slotB = SlotId.Of("SlotB");
        SlotId slotC = SlotId.Of("SlotC");
        SlotId slotD = SlotId.Of("SlotD");
        SlotId slotE = SlotId.Of("SlotE");
        OwnerId alice = OwnerId.Of("Alice");
        OwnerId bob = OwnerId.Of("Bob");
        OwnerId charlie = OwnerId.Of("Charlie");
        OwnerId diana = OwnerId.Of("Diana");
        OwnerId eve = OwnerId.Of("Eve");
        ThereIsSlotOwnedBy(slotA, alice);
        ThereIsSlotOwnedBy(slotB, bob);
        ThereIsSlotOwnedBy(slotC, charlie);
        ThereIsSlotOwnedBy(slotD, diana);
        ThereIsSlotOwnedBy(slotE, eve);

        BatchReservationResult result = UseCase().Execute([
            new ReservationChangeRequest(slotA, slotB, alice),
            new ReservationChangeRequest(slotB, slotC, bob),
            new ReservationChangeRequest(slotC, slotD, charlie),
            new ReservationChangeRequest(slotD, slotE, diana),
            new ReservationChangeRequest(slotE, slotA, eve)
        ]);

        Assert.Equal(BatchReservationResult.Status.SUCCESS, result.GetStatus());
        Assert.Equal(5, result.ExecutedRequests().Count);
        Assert.Equal(eve, FindSlotOwner(slotA));
        Assert.Equal(alice, FindSlotOwner(slotB));
        Assert.Equal(bob, FindSlotOwner(slotC));
        Assert.Equal(charlie, FindSlotOwner(slotD));
        Assert.Equal(diana, FindSlotOwner(slotE));
    }

    [Fact]
    public void ReturnsFailureWhenNoCycle()
    {
        SlotId slotA = SlotId.Of("SlotA");
        SlotId slotB = SlotId.Of("SlotB");
        OwnerId userX = OwnerId.Of("UserX");
        ThereIsSlotOwnedBy(slotA, userX);

        BatchReservationResult result = UseCase().Execute([
            new ReservationChangeRequest(slotA, slotB, userX)
        ]);

        Assert.Equal(BatchReservationResult.Status.FAILURE, result.GetStatus());
        Assert.Empty(result.ExecutedRequests());
        Assert.Equal(userX, FindSlotOwner(slotA));
    }

    private BatchReservationUseCase UseCase() => new(slotRepository);

    private Slot ThereIsSlotOwnedBy(SlotId slotId, OwnerId owner)
    {
        Slot slot = Slot.Create(slotId, owner);
        slotRepository.Save(slot);
        return slot;
    }

    private Slot ThereIsFreeSlot(SlotId slotId) => ThereIsSlotOwnedBy(slotId, OwnerId.Empty());

    private OwnerId FindSlotOwner(SlotId slotId) =>
        slotRepository.FindById(slotId)?.GetOwner() ?? throw new KeyNotFoundException();
}
