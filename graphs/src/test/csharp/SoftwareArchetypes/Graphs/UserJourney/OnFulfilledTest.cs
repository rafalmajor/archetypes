using Xunit;

namespace SoftwareArchetypes.Graphs.UserJourney;

public sealed class OnFulfilledTest
{
    [Fact]
    public void ShouldTransitionToNewStateWhenConditionFulfilled()
    {
        State newLoan = State.Of(Product.NewLoan());
        State afterPayment = State.Of(Product.Penalty());
        Condition paymentOnTime = Condition.PaymentOnTime();
        UserJourney journey = UserJourney.Builder(UserJourneyId.Of("user-1"))
            .From(newLoan).On(paymentOnTime).Goto_(afterPayment)
            .WithCurrentState(newLoan)
            .Build();

        UserJourney updatedJourney = journey.OnFulfilled(paymentOnTime);

        Assert.Equal(afterPayment, updatedJourney.CurrentState());
    }

    [Fact]
    public void ShouldChainMultipleTransitions()
    {
        State state1 = State.Of(Product.NewLoan());
        State state2 = State.Of(Product.Penalty());
        State state3 = State.Of(Product.Discount(10));
        Condition step1 = Condition.PaymentOnTime();
        Condition step2 = Condition.PromotionApproved();
        UserJourney journey = UserJourney.Builder(UserJourneyId.Of("user-2"))
            .From(state1).On(step1).Goto_(state2)
            .From(state2).On(step2).Goto_(state3)
            .WithCurrentState(state1)
            .Build();

        UserJourney afterStep1 = journey.OnFulfilled(step1);
        UserJourney afterStep2 = afterStep1.OnFulfilled(step2);

        Assert.Equal(state2, afterStep1.CurrentState());
        Assert.Equal(state3, afterStep2.CurrentState());
    }

    [Fact]
    public void ShouldReturnSameJourneyWhenConditionNotFound()
    {
        State newLoan = State.Of(Product.NewLoan());
        State afterPayment = State.Of(Product.Penalty());
        Condition paymentOnTime = Condition.PaymentOnTime();
        UserJourney journey = UserJourney.Builder(UserJourneyId.Of("user-3"))
            .From(newLoan).On(paymentOnTime).Goto_(afterPayment)
            .WithCurrentState(newLoan)
            .Build();

        UserJourney result = journey.OnFulfilled(Condition.LatePayments(5));

        Assert.Equal(journey.CurrentState(), result.CurrentState());
    }

    [Fact]
    public void ShouldHandleMultipleOutgoingEdges()
    {
        State newLoan = State.Of(Product.NewLoan());
        State penaltyState = State.Of(Product.Penalty());
        State discountState = State.Of(Product.Discount(5));
        Condition latePayment = Condition.LatePayments(1);
        Condition onTimePayment = Condition.PaymentOnTime();
        UserJourney journey = UserJourney.Builder(UserJourneyId.Of("user-5"))
            .From(newLoan).On(latePayment).Goto_(penaltyState)
            .From(newLoan).On(onTimePayment).Goto_(discountState)
            .WithCurrentState(newLoan)
            .Build();

        UserJourney afterLatePayment = journey.OnFulfilled(latePayment);
        UserJourney afterOnTimePayment = journey.OnFulfilled(onTimePayment);

        Assert.Equal(penaltyState, afterLatePayment.CurrentState());
        Assert.Equal(discountState, afterOnTimePayment.CurrentState());
    }

    [Fact]
    public void ShouldBuildComplexJourneyWithTransitions()
    {
        State newLoan = State.Of(Product.NewLoan());
        State afterPayment1 = State.Of(Product.NewLoan(), Product.Penalty());
        State afterPayment2 = State.Of(Product.Penalty());
        State discountState = State.Of(Product.Discount(10));
        Condition step1 = Condition.PaymentOnTime();
        Condition step2 = Condition.LatePayments(1);
        Condition step3 = Condition.PromotionApproved();
        UserJourney journey = UserJourney.Builder(UserJourneyId.Of("user-7"))
            .From(newLoan).On(step1).Goto_(afterPayment1)
            .From(afterPayment1).On(step2).Goto_(afterPayment2)
            .From(afterPayment2).On(step3).Goto_(discountState)
            .WithCurrentState(newLoan)
            .Build();

        UserJourney finalJourney = journey.OnFulfilled(step1).OnFulfilled(step2).OnFulfilled(step3);

        Assert.Equal(discountState, finalJourney.CurrentState());
    }
}
