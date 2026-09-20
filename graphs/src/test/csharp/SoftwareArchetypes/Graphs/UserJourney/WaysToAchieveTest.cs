using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SoftwareArchetypes.Graphs.UserJourney;

public sealed class WaysToAchieveTest
{
    [Fact]
    public void ShouldFindSimplePath()
    {
        State newLoan = State.Of(Product.NewLoan());
        State penalty = State.Of(Product.Penalty());
        Condition payOnTime6Times = Condition.LatePayments(6);
        UserJourney journey = UserJourney.Builder(UserJourneyId.Of("user-1"))
            .From(newLoan).On(payOnTime6Times).Goto_(penalty)
            .WithCurrentState(newLoan)
            .Build();

        ISet<CustomerPath> paths = journey.WaysToAchieve(Product.ProductType.PENALTY);

        CustomerPath path = Assert.Single(paths);
        Assert.Equal(1, path.Length());
        Assert.Contains(Condition.LatePayments(6), path.Conditions());
    }

    [Fact]
    public void ShouldFindMultiplePathsToDiscount()
    {
        State newLoan = State.Of(Product.NewLoan());
        State afterPayments = State.Of(Product.Penalty());
        State discount10 = State.Of(Product.Discount(10));
        UserJourney journey = UserJourney.Builder(UserJourneyId.Of("user-4"))
            .From(newLoan).On(Condition.PaymentOnTime()).Goto_(discount10)
            .From(newLoan).On(Condition.LatePayments(3)).Goto_(afterPayments)
            .From(afterPayments).On(Condition.PromotionApproved()).Goto_(discount10)
            .WithCurrentState(newLoan)
            .Build();

        ISet<CustomerPath> paths = journey.WaysToAchieve(Product.ProductType.DISCOUNT);

        Assert.Equal(2, paths.Count);
        AssertContainsPath(paths, Condition.PaymentOnTime());
        AssertContainsPath(paths, Condition.LatePayments(3), Condition.PromotionApproved());
    }

    [Fact]
    public void ShouldReturnEmptyWhenRequestedStateIsNotPresent()
    {
        State newLoan = State.Of(Product.NewLoan());
        UserJourney journey = UserJourney.Builder(UserJourneyId.Of("user-5"))
            .From(newLoan).On(Condition.LatePayments(1)).Goto_(State.Of(Product.Penalty()))
            .WithCurrentState(newLoan)
            .Build();

        Assert.Empty(journey.WaysToAchieve(Product.ProductType.DISCOUNT));
    }

    [Fact]
    public void ShouldReturnEmptyWhenStateUnreachable()
    {
        State newLoan = State.Of(Product.NewLoan());
        State penalty = State.Of(Product.Penalty());
        State discount10 = State.Of(Product.Discount(10));
        UserJourney journey = UserJourney.Builder(UserJourneyId.Of("user-7"))
            .From(newLoan).On(Condition.LatePayments(1)).Goto_(penalty)
            .From(newLoan).On(Condition.PaymentOnTime()).Goto_(discount10)
            .WithCurrentState(penalty)
            .Build();

        Assert.Empty(journey.WaysToAchieve(Product.ProductType.DISCOUNT));
    }

    [Fact]
    public void ShouldFindComplexPathThroughMultipleStates()
    {
        State newLoan = State.Of(Product.NewLoan());
        State afterPayment1 = State.Of(Product.NewLoan(), Product.Penalty());
        State afterPayment2 = State.Of(Product.Penalty());
        State discount10 = State.Of(Product.Discount(10));
        UserJourney journey = UserJourney.Builder(UserJourneyId.Of("user-9"))
            .From(newLoan).On(Condition.PaymentOnTime()).Goto_(afterPayment1)
            .From(afterPayment1).On(Condition.LatePayments(1)).Goto_(afterPayment2)
            .From(afterPayment2).On(Condition.PromotionApproved()).Goto_(discount10)
            .WithCurrentState(newLoan)
            .Build();

        CustomerPath path = Assert.Single(journey.WaysToAchieve(Product.ProductType.DISCOUNT));

        Assert.Equal(3, path.Length());
    }

    private static void AssertContainsPath(ISet<CustomerPath> paths, params Condition[] expectedConditions)
    {
        bool found = paths.Any(path => path.Conditions().SequenceEqual(expectedConditions));
        Assert.True(found, $"Expected to find path: {string.Join(" -> ", expectedConditions.Select(FormatCondition))}");
    }

    private static string FormatCondition(Condition condition) => condition.Attributes().Count == 0
        ? condition.Type().ToString()
        : $"{condition.Type()}{condition.Attributes()}";
}
