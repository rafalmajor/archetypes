using Xunit;

namespace SoftwareArchetypes.Graphs.Cycles;

public sealed class EligibilityTest
{
    [Fact]
    public void CanMarkTransferAsEligible()
    {
        Eligibility eligibility = new();
        OwnerId alice = OwnerId.Of("Alice");
        OwnerId bob = OwnerId.Of("Bob");

        eligibility.MarkTransferEligible(alice, bob);

        Assert.True(eligibility.IsTransferEligible(alice, bob));
    }

    [Fact]
    public void TransferIsIneligibleByDefault()
    {
        Eligibility eligibility = new();
        Assert.False(eligibility.IsTransferEligible(OwnerId.Of("Alice"), OwnerId.Of("Bob")));
    }

    [Fact]
    public void CanMarkTransferAsIneligible()
    {
        Eligibility eligibility = new();
        OwnerId alice = OwnerId.Of("Alice");
        OwnerId bob = OwnerId.Of("Bob");
        eligibility.MarkTransferEligible(alice, bob);

        eligibility.MarkTransferIneligible(alice, bob);

        Assert.False(eligibility.IsTransferEligible(alice, bob));
    }

    [Fact]
    public void TransferIsAsymmetric()
    {
        Eligibility eligibility = new();
        OwnerId alice = OwnerId.Of("Alice");
        OwnerId bob = OwnerId.Of("Bob");

        eligibility.MarkTransferEligible(alice, bob);

        Assert.True(eligibility.IsTransferEligible(alice, bob));
        Assert.False(eligibility.IsTransferEligible(bob, alice));
    }

    [Fact]
    public void MultipleTransfersCanBeEligible()
    {
        Eligibility eligibility = new();
        OwnerId alice = OwnerId.Of("Alice");
        OwnerId bob = OwnerId.Of("Bob");
        OwnerId charlie = OwnerId.Of("Charlie");

        eligibility.MarkTransferEligible(alice, bob);
        eligibility.MarkTransferEligible(bob, charlie);
        eligibility.MarkTransferEligible(charlie, alice);

        Assert.True(eligibility.IsTransferEligible(alice, bob));
        Assert.True(eligibility.IsTransferEligible(bob, charlie));
        Assert.True(eligibility.IsTransferEligible(charlie, alice));
    }
}
