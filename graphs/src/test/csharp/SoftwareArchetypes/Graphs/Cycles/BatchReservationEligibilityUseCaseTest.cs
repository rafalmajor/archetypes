using System.Collections.Generic;
using Xunit;

namespace SoftwareArchetypes.Graphs.Cycles;

public sealed class BatchReservationEligibilityUseCaseTest
{
    private readonly SlotRepository slotRepository = new InMemorySlotRepository();

    [Fact]
    public void ExecutesReservationChangesWhenAllUsersEligible()
    {
        SlotId slotA = SlotId.Of("SlotA");
        SlotId slotB = SlotId.Of("SlotB");
        OwnerId userX = OwnerId.Of("UserX");
        OwnerId userY = OwnerId.Of("UserY");
        ThereIsSlotOwnedBy(slotA, userX);
        ThereIsSlotOwnedBy(slotB, userY);
        Eligibility eligibility = EligibilityFor((userX, userY), (userY, userX));

        BatchReservationResult result = UseCase().Execute([
            new ReservationChangeRequest(slotA, slotB, userX),
            new ReservationChangeRequest(slotB, slotA, userY)
        ], eligibility);

        Assert.Equal(BatchReservationResult.Status.SUCCESS, result.GetStatus());
        Assert.Equal(2, result.ExecutedRequests().Count);
        Assert.Equal(userY, FindSlotOwner(slotA));
        Assert.Equal(userX, FindSlotOwner(slotB));
    }

    [Fact]
    public void DoesNotExecuteWhenOneUserNotEligible()
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
        ], EligibilityFor((userX, userY)));

        Assert.Equal(BatchReservationResult.Status.FAILURE, result.GetStatus());
        Assert.Empty(result.ExecutedRequests());
        Assert.Equal(userX, FindSlotOwner(slotA));
        Assert.Equal(userY, FindSlotOwner(slotB));
    }

    [Fact]
    public void ExecutesLongCycleWhenAllEligible()
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
        Eligibility eligibility = EligibilityFor(
            (alice, bob), (bob, charlie), (charlie, diana), (diana, eve), (eve, alice));

        BatchReservationResult result = UseCase().Execute([
            new ReservationChangeRequest(slotA, slotB, alice),
            new ReservationChangeRequest(slotB, slotC, bob),
            new ReservationChangeRequest(slotC, slotD, charlie),
            new ReservationChangeRequest(slotD, slotE, diana),
            new ReservationChangeRequest(slotE, slotA, eve)
        ], eligibility);

        Assert.Equal(BatchReservationResult.Status.SUCCESS, result.GetStatus());
        Assert.Equal(5, result.ExecutedRequests().Count);
        Assert.Equal(eve, FindSlotOwner(slotA));
        Assert.Equal(alice, FindSlotOwner(slotB));
        Assert.Equal(bob, FindSlotOwner(slotC));
        Assert.Equal(charlie, FindSlotOwner(slotD));
        Assert.Equal(diana, FindSlotOwner(slotE));
    }

    [Fact]
    public void DoesNotExecuteCycleWhenOneEdgeIneligible()
    {
        SlotId slotA = SlotId.Of("SlotA");
        SlotId slotB = SlotId.Of("SlotB");
        SlotId slotC = SlotId.Of("SlotC");
        OwnerId alice = OwnerId.Of("Alice");
        OwnerId bob = OwnerId.Of("Bob");
        OwnerId charlie = OwnerId.Of("Charlie");
        ThereIsSlotOwnedBy(slotA, alice);
        ThereIsSlotOwnedBy(slotB, bob);
        ThereIsSlotOwnedBy(slotC, charlie);

        BatchReservationResult result = UseCase().Execute([
            new ReservationChangeRequest(slotA, slotB, alice),
            new ReservationChangeRequest(slotB, slotC, bob),
            new ReservationChangeRequest(slotC, slotA, charlie)
        ], EligibilityFor((alice, bob), (bob, charlie)));

        Assert.Equal(BatchReservationResult.Status.FAILURE, result.GetStatus());
        Assert.Empty(result.ExecutedRequests());
        Assert.Equal(alice, FindSlotOwner(slotA));
        Assert.Equal(bob, FindSlotOwner(slotB));
        Assert.Equal(charlie, FindSlotOwner(slotC));
    }

    [Fact]
    public void CanRevokeEligibility()
    {
        SlotId slotA = SlotId.Of("SlotA");
        SlotId slotB = SlotId.Of("SlotB");
        OwnerId userX = OwnerId.Of("UserX");
        OwnerId userY = OwnerId.Of("UserY");
        ThereIsSlotOwnedBy(slotA, userX);
        ThereIsSlotOwnedBy(slotB, userY);
        Eligibility eligibility = EligibilityFor((userX, userY), (userY, userX));
        eligibility.MarkTransferIneligible(userY, userX);

        BatchReservationResult result = UseCase().Execute([
            new ReservationChangeRequest(slotA, slotB, userX),
            new ReservationChangeRequest(slotB, slotA, userY)
        ], eligibility);

        Assert.Equal(BatchReservationResult.Status.FAILURE, result.GetStatus());
        Assert.Empty(result.ExecutedRequests());
        Assert.Equal(userX, FindSlotOwner(slotA));
        Assert.Equal(userY, FindSlotOwner(slotB));
    }

    private BatchReservationUseCase UseCase() => new(slotRepository);

    private static Eligibility EligibilityFor(params (OwnerId From, OwnerId To)[] transfers)
    {
        Eligibility eligibility = new();
        foreach ((OwnerId from, OwnerId to) in transfers)
        {
            eligibility.MarkTransferEligible(from, to);
        }

        return eligibility;
    }

    private void ThereIsSlotOwnedBy(SlotId slotId, OwnerId owner) =>
        slotRepository.Save(Slot.Create(slotId, owner));

    private OwnerId FindSlotOwner(SlotId slotId) =>
        slotRepository.FindById(slotId)?.GetOwner() ?? throw new KeyNotFoundException();
}
