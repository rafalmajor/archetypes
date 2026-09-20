using System.Collections.Generic;
using Xunit;

namespace com.softwarearchetypes.graphs.cycles;

public sealed class BatchReservationUseCaseTest
{
    private readonly SlotRepository slotRepository = new InMemorySlotRepository();

    [Fact]
    public void executesDependentReservationChangesWhenSlotsRemainValid()
    {
        SlotId slotA = SlotId.of("SlotA");
        SlotId slotB = SlotId.of("SlotB");
        OwnerId userX = OwnerId.of("UserX");
        OwnerId userY = OwnerId.of("UserY");
        thereIsSlotOwnedBy(slotA, userX);
        thereIsSlotOwnedBy(slotB, userY);

        BatchReservationResult result = useCase().execute([
            new ReservationChangeRequest(slotA, slotB, userX),
            new ReservationChangeRequest(slotB, slotA, userY)
        ]);

        Assert.Equal(BatchReservationResult.Status.SUCCESS, result.status());
        Assert.Equal(2, result.executedRequests().Count);
        Assert.Equal(userY, findSlotOwner(slotA));
        Assert.Equal(userX, findSlotOwner(slotB));
    }

    [Fact]
    public void executesDependentReservationChangesAndSkipsInvalidOnes()
    {
        SlotId slotA = SlotId.of("SlotA");
        SlotId slotB = SlotId.of("SlotB");
        SlotId slotD = SlotId.of("SlotD");
        OwnerId userX = OwnerId.of("UserX");
        OwnerId userY = OwnerId.of("UserY");
        OwnerId userZ = OwnerId.of("UserZ");
        thereIsSlotOwnedBy(slotA, userX);
        thereIsSlotOwnedBy(slotB, userY);
        thereIsFreeSlot(slotD);

        BatchReservationResult result = useCase().execute([
            new ReservationChangeRequest(slotB, slotA, userY),
            new ReservationChangeRequest(slotA, slotB, userX),
            new ReservationChangeRequest(slotD, slotA, userZ)
        ]);

        Assert.Equal(BatchReservationResult.Status.SUCCESS, result.status());
        Assert.Equal(2, result.executedRequests().Count);
        Assert.Equal(userY, findSlotOwner(slotA));
        Assert.Equal(userX, findSlotOwner(slotB));
        Assert.Equal(OwnerId.empty(), findSlotOwner(slotD));
    }

    [Fact]
    public void executesComplexDependentReservationChangesWithMultipleSlots()
    {
        SlotId slotA = SlotId.of("SlotA");
        SlotId slotB = SlotId.of("SlotB");
        SlotId slotC = SlotId.of("SlotC");
        SlotId slotD = SlotId.of("SlotD");
        SlotId slotE = SlotId.of("SlotE");
        OwnerId alice = OwnerId.of("Alice");
        OwnerId bob = OwnerId.of("Bob");
        OwnerId charlie = OwnerId.of("Charlie");
        OwnerId diana = OwnerId.of("Diana");
        OwnerId eve = OwnerId.of("Eve");
        thereIsSlotOwnedBy(slotA, alice);
        thereIsSlotOwnedBy(slotB, bob);
        thereIsSlotOwnedBy(slotC, charlie);
        thereIsSlotOwnedBy(slotD, diana);
        thereIsSlotOwnedBy(slotE, eve);

        BatchReservationResult result = useCase().execute([
            new ReservationChangeRequest(slotA, slotB, alice),
            new ReservationChangeRequest(slotB, slotC, bob),
            new ReservationChangeRequest(slotC, slotD, charlie),
            new ReservationChangeRequest(slotD, slotE, diana),
            new ReservationChangeRequest(slotE, slotA, eve)
        ]);

        Assert.Equal(BatchReservationResult.Status.SUCCESS, result.status());
        Assert.Equal(5, result.executedRequests().Count);
        Assert.Equal(eve, findSlotOwner(slotA));
        Assert.Equal(alice, findSlotOwner(slotB));
        Assert.Equal(bob, findSlotOwner(slotC));
        Assert.Equal(charlie, findSlotOwner(slotD));
        Assert.Equal(diana, findSlotOwner(slotE));
    }

    [Fact]
    public void returnsFailureWhenNoCycle()
    {
        SlotId slotA = SlotId.of("SlotA");
        SlotId slotB = SlotId.of("SlotB");
        OwnerId userX = OwnerId.of("UserX");
        thereIsSlotOwnedBy(slotA, userX);

        BatchReservationResult result = useCase().execute([
            new ReservationChangeRequest(slotA, slotB, userX)
        ]);

        Assert.Equal(BatchReservationResult.Status.FAILURE, result.status());
        Assert.Empty(result.executedRequests());
        Assert.Equal(userX, findSlotOwner(slotA));
    }

    private BatchReservationUseCase useCase() => new(slotRepository);

    private Slot thereIsSlotOwnedBy(SlotId slotId, OwnerId owner)
    {
        Slot slot = Slot.create(slotId, owner);
        slotRepository.save(slot);
        return slot;
    }

    private Slot thereIsFreeSlot(SlotId slotId) => thereIsSlotOwnedBy(slotId, OwnerId.empty());

    private OwnerId findSlotOwner(SlotId slotId) =>
        slotRepository.findById(slotId)?.getOwner() ?? throw new KeyNotFoundException();
}
