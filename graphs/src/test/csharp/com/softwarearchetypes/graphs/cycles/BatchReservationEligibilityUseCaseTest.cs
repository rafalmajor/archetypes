using System.Collections.Generic;
using Xunit;

namespace com.softwarearchetypes.graphs.cycles;

public sealed class BatchReservationEligibilityUseCaseTest
{
    private readonly SlotRepository slotRepository = new InMemorySlotRepository();

    [Fact]
    public void executesReservationChangesWhenAllUsersEligible()
    {
        SlotId slotA = SlotId.of("SlotA");
        SlotId slotB = SlotId.of("SlotB");
        OwnerId userX = OwnerId.of("UserX");
        OwnerId userY = OwnerId.of("UserY");
        thereIsSlotOwnedBy(slotA, userX);
        thereIsSlotOwnedBy(slotB, userY);
        Eligibility eligibility = eligibilityFor((userX, userY), (userY, userX));

        BatchReservationResult result = useCase().execute([
            new ReservationChangeRequest(slotA, slotB, userX),
            new ReservationChangeRequest(slotB, slotA, userY)
        ], eligibility);

        Assert.Equal(BatchReservationResult.Status.SUCCESS, result.status());
        Assert.Equal(2, result.executedRequests().Count);
        Assert.Equal(userY, findSlotOwner(slotA));
        Assert.Equal(userX, findSlotOwner(slotB));
    }

    [Fact]
    public void doesNotExecuteWhenOneUserNotEligible()
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
        ], eligibilityFor((userX, userY)));

        Assert.Equal(BatchReservationResult.Status.FAILURE, result.status());
        Assert.Empty(result.executedRequests());
        Assert.Equal(userX, findSlotOwner(slotA));
        Assert.Equal(userY, findSlotOwner(slotB));
    }

    [Fact]
    public void executesLongCycleWhenAllEligible()
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
        Eligibility eligibility = eligibilityFor(
            (alice, bob), (bob, charlie), (charlie, diana), (diana, eve), (eve, alice));

        BatchReservationResult result = useCase().execute([
            new ReservationChangeRequest(slotA, slotB, alice),
            new ReservationChangeRequest(slotB, slotC, bob),
            new ReservationChangeRequest(slotC, slotD, charlie),
            new ReservationChangeRequest(slotD, slotE, diana),
            new ReservationChangeRequest(slotE, slotA, eve)
        ], eligibility);

        Assert.Equal(BatchReservationResult.Status.SUCCESS, result.status());
        Assert.Equal(5, result.executedRequests().Count);
        Assert.Equal(eve, findSlotOwner(slotA));
        Assert.Equal(alice, findSlotOwner(slotB));
        Assert.Equal(bob, findSlotOwner(slotC));
        Assert.Equal(charlie, findSlotOwner(slotD));
        Assert.Equal(diana, findSlotOwner(slotE));
    }

    [Fact]
    public void doesNotExecuteCycleWhenOneEdgeIneligible()
    {
        SlotId slotA = SlotId.of("SlotA");
        SlotId slotB = SlotId.of("SlotB");
        SlotId slotC = SlotId.of("SlotC");
        OwnerId alice = OwnerId.of("Alice");
        OwnerId bob = OwnerId.of("Bob");
        OwnerId charlie = OwnerId.of("Charlie");
        thereIsSlotOwnedBy(slotA, alice);
        thereIsSlotOwnedBy(slotB, bob);
        thereIsSlotOwnedBy(slotC, charlie);

        BatchReservationResult result = useCase().execute([
            new ReservationChangeRequest(slotA, slotB, alice),
            new ReservationChangeRequest(slotB, slotC, bob),
            new ReservationChangeRequest(slotC, slotA, charlie)
        ], eligibilityFor((alice, bob), (bob, charlie)));

        Assert.Equal(BatchReservationResult.Status.FAILURE, result.status());
        Assert.Empty(result.executedRequests());
        Assert.Equal(alice, findSlotOwner(slotA));
        Assert.Equal(bob, findSlotOwner(slotB));
        Assert.Equal(charlie, findSlotOwner(slotC));
    }

    [Fact]
    public void canRevokeEligibility()
    {
        SlotId slotA = SlotId.of("SlotA");
        SlotId slotB = SlotId.of("SlotB");
        OwnerId userX = OwnerId.of("UserX");
        OwnerId userY = OwnerId.of("UserY");
        thereIsSlotOwnedBy(slotA, userX);
        thereIsSlotOwnedBy(slotB, userY);
        Eligibility eligibility = eligibilityFor((userX, userY), (userY, userX));
        eligibility.markTransferIneligible(userY, userX);

        BatchReservationResult result = useCase().execute([
            new ReservationChangeRequest(slotA, slotB, userX),
            new ReservationChangeRequest(slotB, slotA, userY)
        ], eligibility);

        Assert.Equal(BatchReservationResult.Status.FAILURE, result.status());
        Assert.Empty(result.executedRequests());
        Assert.Equal(userX, findSlotOwner(slotA));
        Assert.Equal(userY, findSlotOwner(slotB));
    }

    private BatchReservationUseCase useCase() => new(slotRepository);

    private static Eligibility eligibilityFor(params (OwnerId From, OwnerId To)[] transfers)
    {
        Eligibility eligibility = new();
        foreach ((OwnerId from, OwnerId to) in transfers)
        {
            eligibility.markTransferEligible(from, to);
        }

        return eligibility;
    }

    private void thereIsSlotOwnedBy(SlotId slotId, OwnerId owner) =>
        slotRepository.save(Slot.create(slotId, owner));

    private OwnerId findSlotOwner(SlotId slotId) =>
        slotRepository.findById(slotId)?.getOwner() ?? throw new KeyNotFoundException();
}