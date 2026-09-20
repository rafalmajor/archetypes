using Xunit;

namespace com.softwarearchetypes.graphs.userjourney;

public sealed class OnFulfilledTest
{
    [Fact]
    public void shouldTransitionToNewStateWhenConditionFulfilled()
    {
        State newLoan = State.of(Product.newLoan());
        State afterPayment = State.of(Product.penalty());
        Condition paymentOnTime = Condition.paymentOnTime();
        UserJourney journey = UserJourney.builder(UserJourneyId.of("user-1"))
            .from(newLoan).on(paymentOnTime).goto_(afterPayment)
            .withCurrentState(newLoan)
            .build();

        UserJourney updatedJourney = journey.onFulfilled(paymentOnTime);

        Assert.Equal(afterPayment, updatedJourney.currentState());
    }

    [Fact]
    public void shouldChainMultipleTransitions()
    {
        State state1 = State.of(Product.newLoan());
        State state2 = State.of(Product.penalty());
        State state3 = State.of(Product.discount(10));
        Condition step1 = Condition.paymentOnTime();
        Condition step2 = Condition.promotionApproved();
        UserJourney journey = UserJourney.builder(UserJourneyId.of("user-2"))
            .from(state1).on(step1).goto_(state2)
            .from(state2).on(step2).goto_(state3)
            .withCurrentState(state1)
            .build();

        UserJourney afterStep1 = journey.onFulfilled(step1);
        UserJourney afterStep2 = afterStep1.onFulfilled(step2);

        Assert.Equal(state2, afterStep1.currentState());
        Assert.Equal(state3, afterStep2.currentState());
    }

    [Fact]
    public void shouldReturnSameJourneyWhenConditionNotFound()
    {
        State newLoan = State.of(Product.newLoan());
        State afterPayment = State.of(Product.penalty());
        Condition paymentOnTime = Condition.paymentOnTime();
        UserJourney journey = UserJourney.builder(UserJourneyId.of("user-3"))
            .from(newLoan).on(paymentOnTime).goto_(afterPayment)
            .withCurrentState(newLoan)
            .build();

        UserJourney result = journey.onFulfilled(Condition.latePayments(5));

        Assert.Equal(journey.currentState(), result.currentState());
    }

    [Fact]
    public void shouldHandleMultipleOutgoingEdges()
    {
        State newLoan = State.of(Product.newLoan());
        State penaltyState = State.of(Product.penalty());
        State discountState = State.of(Product.discount(5));
        Condition latePayment = Condition.latePayments(1);
        Condition onTimePayment = Condition.paymentOnTime();
        UserJourney journey = UserJourney.builder(UserJourneyId.of("user-5"))
            .from(newLoan).on(latePayment).goto_(penaltyState)
            .from(newLoan).on(onTimePayment).goto_(discountState)
            .withCurrentState(newLoan)
            .build();

        UserJourney afterLatePayment = journey.onFulfilled(latePayment);
        UserJourney afterOnTimePayment = journey.onFulfilled(onTimePayment);

        Assert.Equal(penaltyState, afterLatePayment.currentState());
        Assert.Equal(discountState, afterOnTimePayment.currentState());
    }

    [Fact]
    public void shouldBuildComplexJourneyWithTransitions()
    {
        State newLoan = State.of(Product.newLoan());
        State afterPayment1 = State.of(Product.newLoan(), Product.penalty());
        State afterPayment2 = State.of(Product.penalty());
        State discountState = State.of(Product.discount(10));
        Condition step1 = Condition.paymentOnTime();
        Condition step2 = Condition.latePayments(1);
        Condition step3 = Condition.promotionApproved();
        UserJourney journey = UserJourney.builder(UserJourneyId.of("user-7"))
            .from(newLoan).on(step1).goto_(afterPayment1)
            .from(afterPayment1).on(step2).goto_(afterPayment2)
            .from(afterPayment2).on(step3).goto_(discountState)
            .withCurrentState(newLoan)
            .build();

        UserJourney finalJourney = journey.onFulfilled(step1).onFulfilled(step2).onFulfilled(step3);

        Assert.Equal(discountState, finalJourney.currentState());
    }
}
