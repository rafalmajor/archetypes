using Xunit;

namespace com.softwarearchetypes.graphs.cycles;

public sealed class EligibilityTest
{
    [Fact]
    public void canMarkTransferAsEligible()
    {
        Eligibility eligibility = new();
        OwnerId alice = OwnerId.of("Alice");
        OwnerId bob = OwnerId.of("Bob");

        eligibility.markTransferEligible(alice, bob);

        Assert.True(eligibility.isTransferEligible(alice, bob));
    }

    [Fact]
    public void transferIsIneligibleByDefault()
    {
        Eligibility eligibility = new();
        Assert.False(eligibility.isTransferEligible(OwnerId.of("Alice"), OwnerId.of("Bob")));
    }

    [Fact]
    public void canMarkTransferAsIneligible()
    {
        Eligibility eligibility = new();
        OwnerId alice = OwnerId.of("Alice");
        OwnerId bob = OwnerId.of("Bob");
        eligibility.markTransferEligible(alice, bob);

        eligibility.markTransferIneligible(alice, bob);

        Assert.False(eligibility.isTransferEligible(alice, bob));
    }

    [Fact]
    public void transferIsAsymmetric()
    {
        Eligibility eligibility = new();
        OwnerId alice = OwnerId.of("Alice");
        OwnerId bob = OwnerId.of("Bob");

        eligibility.markTransferEligible(alice, bob);

        Assert.True(eligibility.isTransferEligible(alice, bob));
        Assert.False(eligibility.isTransferEligible(bob, alice));
    }

    [Fact]
    public void multipleTransfersCanBeEligible()
    {
        Eligibility eligibility = new();
        OwnerId alice = OwnerId.of("Alice");
        OwnerId bob = OwnerId.of("Bob");
        OwnerId charlie = OwnerId.of("Charlie");

        eligibility.markTransferEligible(alice, bob);
        eligibility.markTransferEligible(bob, charlie);
        eligibility.markTransferEligible(charlie, alice);

        Assert.True(eligibility.isTransferEligible(alice, bob));
        Assert.True(eligibility.isTransferEligible(bob, charlie));
        Assert.True(eligibility.isTransferEligible(charlie, alice));
    }
}