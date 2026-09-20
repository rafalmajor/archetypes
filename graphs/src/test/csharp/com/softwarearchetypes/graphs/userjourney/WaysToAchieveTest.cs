using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace com.softwarearchetypes.graphs.userjourney;

public sealed class WaysToAchieveTest
{
    [Fact]
    public void shouldFindSimplePath()
    {
        State newLoan = State.of(Product.newLoan());
        State penalty = State.of(Product.penalty());
        Condition payOnTime6Times = Condition.latePayments(6);
        UserJourney journey = UserJourney.builder(UserJourneyId.of("user-1"))
            .from(newLoan).on(payOnTime6Times).goto_(penalty)
            .withCurrentState(newLoan)
            .build();

        ISet<CustomerPath> paths = journey.waysToAchieve(Product.ProductType.PENALTY);

        CustomerPath path = Assert.Single(paths);
        Assert.Equal(1, path.length());
        Assert.Contains(Condition.latePayments(6), path.conditions());
    }

    [Fact]
    public void shouldFindMultiplePathsToDiscount()
    {
        State newLoan = State.of(Product.newLoan());
        State afterPayments = State.of(Product.penalty());
        State discount10 = State.of(Product.discount(10));
        UserJourney journey = UserJourney.builder(UserJourneyId.of("user-4"))
            .from(newLoan).on(Condition.paymentOnTime()).goto_(discount10)
            .from(newLoan).on(Condition.latePayments(3)).goto_(afterPayments)
            .from(afterPayments).on(Condition.promotionApproved()).goto_(discount10)
            .withCurrentState(newLoan)
            .build();

        ISet<CustomerPath> paths = journey.waysToAchieve(Product.ProductType.DISCOUNT);

        Assert.Equal(2, paths.Count);
        assertContainsPath(paths, Condition.paymentOnTime());
        assertContainsPath(paths, Condition.latePayments(3), Condition.promotionApproved());
    }

    [Fact]
    public void shouldReturnEmptyWhenRequestedStateIsNotPresent()
    {
        State newLoan = State.of(Product.newLoan());
        UserJourney journey = UserJourney.builder(UserJourneyId.of("user-5"))
            .from(newLoan).on(Condition.latePayments(1)).goto_(State.of(Product.penalty()))
            .withCurrentState(newLoan)
            .build();

        Assert.Empty(journey.waysToAchieve(Product.ProductType.DISCOUNT));
    }

    [Fact]
    public void shouldReturnEmptyWhenStateUnreachable()
    {
        State newLoan = State.of(Product.newLoan());
        State penalty = State.of(Product.penalty());
        State discount10 = State.of(Product.discount(10));
        UserJourney journey = UserJourney.builder(UserJourneyId.of("user-7"))
            .from(newLoan).on(Condition.latePayments(1)).goto_(penalty)
            .from(newLoan).on(Condition.paymentOnTime()).goto_(discount10)
            .withCurrentState(penalty)
            .build();

        Assert.Empty(journey.waysToAchieve(Product.ProductType.DISCOUNT));
    }

    [Fact]
    public void shouldFindComplexPathThroughMultipleStates()
    {
        State newLoan = State.of(Product.newLoan());
        State afterPayment1 = State.of(Product.newLoan(), Product.penalty());
        State afterPayment2 = State.of(Product.penalty());
        State discount10 = State.of(Product.discount(10));
        UserJourney journey = UserJourney.builder(UserJourneyId.of("user-9"))
            .from(newLoan).on(Condition.paymentOnTime()).goto_(afterPayment1)
            .from(afterPayment1).on(Condition.latePayments(1)).goto_(afterPayment2)
            .from(afterPayment2).on(Condition.promotionApproved()).goto_(discount10)
            .withCurrentState(newLoan)
            .build();

        CustomerPath path = Assert.Single(journey.waysToAchieve(Product.ProductType.DISCOUNT));

        Assert.Equal(3, path.length());
    }

    private static void assertContainsPath(ISet<CustomerPath> paths, params Condition[] expectedConditions)
    {
        bool found = paths.Any(path => path.conditions().SequenceEqual(expectedConditions));
        Assert.True(found, $"Expected to find path: {string.Join(" -> ", expectedConditions.Select(formatCondition))}");
    }

    private static string formatCondition(Condition condition) => condition.attributes().Count == 0
        ? condition.type().ToString()
        : $"{condition.type()}{condition.attributes()}";
}
