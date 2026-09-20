using Xunit;

namespace SoftwareArchetypes.Graphs.UserJourney;

public sealed class WeightedPathsTest
{
    [Fact]
    public void ShouldFindCheapestPathByMinimizingCost()
    {
        State newLoan = State.Of(Product.NewLoan());
        State intermediate = State.Of(Product.Penalty());
        State discount = State.Of(Product.Discount(10));
        Condition directExpensive = Condition.WithCost(Condition.ConditionType.PAYMENT_ON_TIME, 100.0);
        Condition cheapStep1 = Condition.WithCost(Condition.ConditionType.LATE_PAYMENT, 30.0);
        Condition cheapStep2 = Condition.WithCost(Condition.ConditionType.RESTRUCTURING, 20.0);
        UserJourney journey = UserJourney.Builder(UserJourneyId.Of("user-1"))
            .From(newLoan).On(directExpensive).Goto_(discount)
            .From(newLoan).On(cheapStep1).Goto_(intermediate)
            .From(intermediate).On(cheapStep2).Goto_(discount)
            .WithCurrentState(newLoan)
            .Build();

        CustomerPath? cheapestPath = journey.OptimizedWayToAchieve(
            Product.ProductType.DISCOUNT,
            condition => condition.GetCost());

        Assert.NotNull(cheapestPath);
        Assert.Equal(2, cheapestPath.Length());
        Assert.Contains(cheapStep1, cheapestPath.Conditions());
        Assert.Contains(cheapStep2, cheapestPath.Conditions());
    }

    [Fact]
    public void ShouldFindFastestPathByMinimizingTime()
    {
        State newLoan = State.Of(Product.NewLoan());
        State intermediate1 = State.Of(Product.Penalty());
        State intermediate2 = State.Of(Product.NewLoan(), Product.Penalty());
        State discount = State.Of(Product.Discount(10));
        Condition fastPath = Condition.WithTime(Condition.ConditionType.PAYMENT_ON_TIME, 5);
        Condition slowStep1 = Condition.WithTime(Condition.ConditionType.LATE_PAYMENT, 15);
        Condition slowStep2 = Condition.WithTime(Condition.ConditionType.RESTRUCTURING, 10);
        Condition mediumStep1 = Condition.WithTime(Condition.ConditionType.PROMOTION_APPROVED, 7);
        Condition mediumStep2 = Condition.WithTime(Condition.ConditionType.PAYMENT_ON_TIME, 4);
        UserJourney journey = UserJourney.Builder(UserJourneyId.Of("user-2"))
            .From(newLoan).On(fastPath).Goto_(discount)
            .From(newLoan).On(slowStep1).Goto_(intermediate1)
            .From(intermediate1).On(slowStep2).Goto_(discount)
            .From(newLoan).On(mediumStep1).Goto_(intermediate2)
            .From(intermediate2).On(mediumStep2).Goto_(discount)
            .WithCurrentState(newLoan)
            .Build();

        CustomerPath? fastestPath = journey.OptimizedWayToAchieve(
            Product.ProductType.DISCOUNT,
            condition => condition.GetTime());

        Assert.NotNull(fastestPath);
        Assert.Equal(1, fastestPath.Length());
        Assert.Contains(fastPath, fastestPath.Conditions());
    }

    [Fact]
    public void ShouldDemonstrateTradeoffBetweenCostAndTime()
    {
        State newLoan = State.Of(Product.NewLoan());
        State expressRoute = State.Of(Product.Penalty());
        State economyRoute = State.Of(Product.Discount(5));
        State discount10 = State.Of(Product.Discount(10));
        Condition expressStep1 = Condition.WithAttributes(
            Condition.ConditionType.PAYMENT_ON_TIME, 150.0, 3, 0.0);
        Condition economyStep1 = Condition.WithAttributes(
            Condition.ConditionType.LATE_PAYMENT, 20.0, 30, 0.0);
        Condition toDiscount1 = Condition.WithAttributes(
            Condition.ConditionType.PROMOTION_APPROVED, 10.0, 1, 0.0);
        Condition toDiscount2 = Condition.WithAttributes(
            Condition.ConditionType.RESTRUCTURING, 10.0, 1, 0.0);
        UserJourney journey = UserJourney.Builder(UserJourneyId.Of("user-4"))
            .From(newLoan).On(expressStep1).Goto_(expressRoute)
            .From(expressRoute).On(toDiscount1).Goto_(discount10)
            .From(newLoan).On(economyStep1).Goto_(economyRoute)
            .From(economyRoute).On(toDiscount2).Goto_(discount10)
            .WithCurrentState(newLoan)
            .Build();

        CustomerPath? cheapest = journey.OptimizedWayToAchieve(
            Product.ProductType.DISCOUNT,
            condition => condition.GetCost());
        CustomerPath? fastest = journey.OptimizedWayToAchieve(
            Product.ProductType.DISCOUNT,
            condition => condition.GetTime());

        Assert.NotNull(cheapest);
        Assert.NotNull(fastest);
        Assert.Contains(economyStep1, cheapest.Conditions());
        Assert.Contains(expressStep1, fastest.Conditions());
    }
}
