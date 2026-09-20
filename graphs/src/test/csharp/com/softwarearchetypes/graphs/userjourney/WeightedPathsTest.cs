using Xunit;

namespace com.softwarearchetypes.graphs.userjourney;

public sealed class WeightedPathsTest
{
    [Fact]
    public void shouldFindCheapestPathByMinimizingCost()
    {
        State newLoan = State.of(Product.newLoan());
        State intermediate = State.of(Product.penalty());
        State discount = State.of(Product.discount(10));
        Condition directExpensive = Condition.withCost(Condition.ConditionType.PAYMENT_ON_TIME, 100.0);
        Condition cheapStep1 = Condition.withCost(Condition.ConditionType.LATE_PAYMENT, 30.0);
        Condition cheapStep2 = Condition.withCost(Condition.ConditionType.RESTRUCTURING, 20.0);
        UserJourney journey = UserJourney.builder(UserJourneyId.of("user-1"))
            .from(newLoan).on(directExpensive).goto_(discount)
            .from(newLoan).on(cheapStep1).goto_(intermediate)
            .from(intermediate).on(cheapStep2).goto_(discount)
            .withCurrentState(newLoan)
            .build();

        CustomerPath? cheapestPath = journey.optimizedWayToAchieve(
            Product.ProductType.DISCOUNT,
            condition => condition.getCost());

        Assert.NotNull(cheapestPath);
        Assert.Equal(2, cheapestPath.length());
        Assert.Contains(cheapStep1, cheapestPath.conditions());
        Assert.Contains(cheapStep2, cheapestPath.conditions());
    }

    [Fact]
    public void shouldFindFastestPathByMinimizingTime()
    {
        State newLoan = State.of(Product.newLoan());
        State intermediate1 = State.of(Product.penalty());
        State intermediate2 = State.of(Product.newLoan(), Product.penalty());
        State discount = State.of(Product.discount(10));
        Condition fastPath = Condition.withTime(Condition.ConditionType.PAYMENT_ON_TIME, 5);
        Condition slowStep1 = Condition.withTime(Condition.ConditionType.LATE_PAYMENT, 15);
        Condition slowStep2 = Condition.withTime(Condition.ConditionType.RESTRUCTURING, 10);
        Condition mediumStep1 = Condition.withTime(Condition.ConditionType.PROMOTION_APPROVED, 7);
        Condition mediumStep2 = Condition.withTime(Condition.ConditionType.PAYMENT_ON_TIME, 4);
        UserJourney journey = UserJourney.builder(UserJourneyId.of("user-2"))
            .from(newLoan).on(fastPath).goto_(discount)
            .from(newLoan).on(slowStep1).goto_(intermediate1)
            .from(intermediate1).on(slowStep2).goto_(discount)
            .from(newLoan).on(mediumStep1).goto_(intermediate2)
            .from(intermediate2).on(mediumStep2).goto_(discount)
            .withCurrentState(newLoan)
            .build();

        CustomerPath? fastestPath = journey.optimizedWayToAchieve(
            Product.ProductType.DISCOUNT,
            condition => condition.getTime());

        Assert.NotNull(fastestPath);
        Assert.Equal(1, fastestPath.length());
        Assert.Contains(fastPath, fastestPath.conditions());
    }

    [Fact]
    public void shouldDemonstrateTradeoffBetweenCostAndTime()
    {
        State newLoan = State.of(Product.newLoan());
        State expressRoute = State.of(Product.penalty());
        State economyRoute = State.of(Product.discount(5));
        State discount10 = State.of(Product.discount(10));
        Condition expressStep1 = Condition.withAttributes(
            Condition.ConditionType.PAYMENT_ON_TIME, 150.0, 3, 0.0);
        Condition economyStep1 = Condition.withAttributes(
            Condition.ConditionType.LATE_PAYMENT, 20.0, 30, 0.0);
        Condition toDiscount1 = Condition.withAttributes(
            Condition.ConditionType.PROMOTION_APPROVED, 10.0, 1, 0.0);
        Condition toDiscount2 = Condition.withAttributes(
            Condition.ConditionType.RESTRUCTURING, 10.0, 1, 0.0);
        UserJourney journey = UserJourney.builder(UserJourneyId.of("user-4"))
            .from(newLoan).on(expressStep1).goto_(expressRoute)
            .from(expressRoute).on(toDiscount1).goto_(discount10)
            .from(newLoan).on(economyStep1).goto_(economyRoute)
            .from(economyRoute).on(toDiscount2).goto_(discount10)
            .withCurrentState(newLoan)
            .build();

        CustomerPath? cheapest = journey.optimizedWayToAchieve(
            Product.ProductType.DISCOUNT,
            condition => condition.getCost());
        CustomerPath? fastest = journey.optimizedWayToAchieve(
            Product.ProductType.DISCOUNT,
            condition => condition.getTime());

        Assert.NotNull(cheapest);
        Assert.NotNull(fastest);
        Assert.Contains(economyStep1, cheapest.conditions());
        Assert.Contains(expressStep1, fastest.conditions());
    }
}
