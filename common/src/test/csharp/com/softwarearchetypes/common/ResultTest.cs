using System;
using System.Collections.Generic;
using Xunit;

namespace com.softwarearchetypes.common;

public sealed class ResultTest
{
    [Fact] public void shouldBeMarkedAsSuccessForSuccessResult() => Assert.True(Result.success<string, string>("value").success());
    [Fact] public void shouldNotBeMarkedAsFailureForSuccessResult() => Assert.False(Result.success<string, string>("value").failure());
    [Fact] public void shouldBeMarkedAsFailureForFailureResult() => Assert.True(Result.failure<string, string>("error").failure());
    [Fact] public void shouldNotBeMarkedAsSuccessForFailureResult() => Assert.False(Result.failure<string, string>("error").success());
    [Fact] public void shouldFailToGetSuccessOnFailureResult() => Assert.Throws<InvalidOperationException>(() => Result.failure<string, string>("error").getSuccess());
    [Fact] public void shouldFailToGetFailureOnSuccessResult() => Assert.Throws<InvalidOperationException>(() => Result.success<string, string>("value").getFailure());

    [Fact]
    public void shouldChooseAndProperlyApplySuccessMappingFunction() =>
        Assert.Equal("SUCCESS-value", Result.success<string, string>("value").ifSuccessOrElse(value => "SUCCESS-" + value, value => "FAILURE-" + value));

    [Fact]
    public void shouldChooseAndProperlyApplyFailureMappingFunction() =>
        Assert.Equal("FAILURE-error", Result.failure<string, string>("error").ifSuccessOrElse(value => "SUCCESS-" + value, value => "FAILURE-" + value));

    [Fact]
    public void shouldMapSuccessResultAccordingToMappingFunction() =>
        Assert.Equal("2", Result.success<int, int>(1).biMap(value => (value * 2).ToString(), _ => "").getSuccess());

    [Fact]
    public void shouldMapFailureResultAccordingToMappingFunction() =>
        Assert.Equal("", Result.failure<int, int>(1).biMap(value => (value * 2).ToString(), _ => "").getFailure());

    [Fact]
    public void shouldCallSuccessConsumerOnPeek()
    {
        int successCalls = 0;
        int failureCalls = 0;
        Result<int, int> result = Result.success<int, int>(7);
        Result<int, int> peekResult = result.peek(_ => successCalls++, _ => failureCalls++);
        Assert.Same(result, peekResult);
        Assert.Equal(1, successCalls);
        Assert.Equal(0, failureCalls);
    }

    [Fact]
    public void shouldCallSuccessConsumerOnPeekSuccess()
    {
        int successCalls = 0;
        int failureCalls = 0;
        Result<int, int> result = Result.success<int, int>(7);
        Result<int, int> peekResult = result.peekSuccess(_ => successCalls++).peekFailure(_ => failureCalls++);
        Assert.Same(result, peekResult);
        Assert.Equal(1, successCalls);
        Assert.Equal(0, failureCalls);
    }

    [Fact]
    public void shouldCallFailureConsumerOnPeek()
    {
        int successCalls = 0;
        int failureCalls = 0;
        Result<int, int> result = Result.failure<int, int>(7);
        Result<int, int> peekResult = result.peek(_ => successCalls++, _ => failureCalls++);
        Assert.Same(result, peekResult);
        Assert.Equal(0, successCalls);
        Assert.Equal(1, failureCalls);
    }

    [Fact]
    public void shouldCallFailureConsumerOnPeekFailure()
    {
        int successCalls = 0;
        int failureCalls = 0;
        Result<int, int> result = Result.failure<int, int>(7);
        Result<int, int> peekResult = result.peekSuccess(_ => successCalls++).peekFailure(_ => failureCalls++);
        Assert.Same(result, peekResult);
        Assert.Equal(0, successCalls);
        Assert.Equal(1, failureCalls);
    }

    [Fact]
    public void shouldCombineTwoSuccessResults()
    {
        Result<int?, int> result = Result.success<int?, int>(5).combine(Result.success<int?, int>(3), (left, right) => left - right, (left, right) => left + right);
        Assert.Equal(8, result.getSuccess());
    }

    [Fact]
    public void shouldCombineTwoFailureResults()
    {
        Result<int?, int> result = Result.failure<int?, int>(5).combine(Result.failure<int?, int>(3), (left, right) => left - right, (left, right) => left + right);
        Assert.Equal(2, result.getFailure());
    }

    [Fact]
    public void shouldProduceFailureResultWhenCombiningFailureAndSuccessResults()
    {
        Result<int?, int> success = Result.success<int?, int>(5);
        Result<int?, int> failure = Result.failure<int?, int>(3);
        Func<int?, int?, int?> combiner = (left, right) => (left ?? 0) - (right ?? 0);
        Assert.Equal(-3, success.combine(failure, combiner, (left, right) => left + right).getFailure());
        Assert.Equal(3, failure.combine(success, combiner, (left, right) => left + right).getFailure());
    }

    [Fact] public void shouldMapSuccessValueUsingMapFunction() => Assert.Equal("Value: 20", Result.success<string, int>(10).map(value => $"Value: {value * 2}").getSuccess());
    [Fact] public void shouldNotMapFailureValueUsingMapFunction() => Assert.Equal("Error occurred", Result.failure<string, int>("Error occurred").map(value => $"Value: {value * 2}").getFailure());
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenMapWithNullMapper() => Assert.Throws<ArgumentException>(() => Result.success<string, int>(10).map<string>(null));
    [Fact] public void shouldMapFailureValueUsingMapFailureFunction() => Assert.Equal("Error 404: Not Found", Result.failure<int, int>(404).mapFailure(code => $"Error {code}: Not Found").getFailure());
    [Fact] public void shouldNotMapSuccessValueUsingMapFailureFunction() => Assert.Equal(42, Result.success<int, int>(42).mapFailure(code => $"Error {code}").getSuccess());
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenMapFailureWithNullMapper() => Assert.Throws<ArgumentException>(() => Result.failure<string, int>("error").mapFailure<int>(null));
    [Fact] public void shouldFlatMapSuccessResultWithAnotherSuccessResult() => Assert.Equal(10, Result.success<string, int>(5).flatMap(value => Result.success<string, int>(value * 2)).getSuccess());
    [Fact] public void shouldFlatMapSuccessResultWithFailureResult() => Assert.Equal("Validation failed", Result.success<string, int>(5).flatMap(_ => Result.failure<string, int>("Validation failed")).getFailure());
    [Fact] public void shouldNotFlatMapFailureResult() => Assert.Equal("Initial error", Result.failure<string, int>("Initial error").flatMap(value => Result.success<string, int>(value * 2)).getFailure());
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenFlatMapWithNullMapper() => Assert.Throws<ArgumentException>(() => Result.success<string, int>(10).flatMap<string>(null));
    [Fact] public void shouldFoldSuccessResultUsingRightMapper() => Assert.Equal(30, Result.success<string, int>(10).fold(_ => -1, value => value * 3));
    [Fact] public void shouldFoldFailureResultUsingLeftMapper() => Assert.Equal(5, Result.failure<string, int>("Error").fold(error => error.Length, value => value * 3));
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenFoldingWithNullLeftMapper() => Assert.Throws<ArgumentException>(() => Result.success<string, int>(10).fold<int>(null, value => value * 2));
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenFoldingWithNullRightMapper() => Assert.Throws<ArgumentException>(() => Result.success<string, int>(10).fold<int>(_ => -1, null));
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenBiMapWithNullSuccessMapper() => Assert.Throws<ArgumentException>(() => Result.success<string, int>(10).biMap<string, string>(null, _ => ""));
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenBiMapWithNullFailureMapper() => Assert.Throws<ArgumentException>(() => Result.success<string, int>(10).biMap<string, string>(_ => "", null));
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenIfSuccessOrElseWithNullSuccessMapping() => Assert.Throws<ArgumentException>(() => Result.success<string, int>(10).ifSuccessOrElse<string>(null, _ => ""));
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenIfSuccessOrElseWithNullFailureMapping() => Assert.Throws<ArgumentException>(() => Result.success<string, int>(10).ifSuccessOrElse<string>(_ => "", null));
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenPeekWithNullSuccessConsumer() => Assert.Throws<ArgumentException>(() => Result.success<string, int>(10).peek(null, _ => { }));
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenPeekWithNullFailureConsumer() => Assert.Throws<ArgumentException>(() => Result.success<string, int>(10).peek(_ => { }, null));
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenPeekSuccessWithNullConsumer() => Assert.Throws<ArgumentException>(() => Result.success<string, int>(10).peekSuccess(null));
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenPeekFailureWithNullConsumer() => Assert.Throws<ArgumentException>(() => Result.failure<string, int>("error").peekFailure(null));
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenCombineWithNullSecondResult() => Assert.Throws<ArgumentException>(() => Result.success<string, int>(10).combine<string, int>(null, (_, _) => "", (left, right) => left + right));
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenCombineWithNullFailureCombiner() => Assert.Throws<ArgumentException>(() => Result.success<string, int>(10).combine<string, int>(Result.success<string, int>(20), null, (left, right) => left + right));
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenCombineWithNullSuccessCombiner() => Assert.Throws<ArgumentException>(() => Result.success<string, int>(10).combine<string, int>(Result.success<string, int>(20), (_, _) => "", null));

    [Fact] public void shouldCreateEmptyCompositeResult() => Assert.Empty(Result.composite<string, int>().toResult().getSuccess());
    [Fact] public void shouldCreateEmptyCompositeSetResult() => Assert.Empty(Result.compositeSet<string, int>().toResult().getSuccess());

    [Fact]
    public void shouldAccumulateSuccessResultsIntoList() =>
        Assert.Equal([1, 2, 3], Result.composite<string, int>().accumulate(Result.success<string, int>(1)).accumulate(Result.success<string, int>(2)).accumulate(Result.success<string, int>(3)).toResult().getSuccess());

    [Fact]
    public void shouldAccumulateSuccessResultsIntoSet() =>
        Assert.True(Result.compositeSet<string, int>().accumulate(Result.success<string, int>(1)).accumulate(Result.success<string, int>(2)).accumulate(Result.success<string, int>(3)).toResult().getSuccess().SetEquals([1, 2, 3]));

    [Fact] public void shouldStopAccumulatingOnFirstFailure() => Assert.Equal("Error occurred", Result.composite<string, int>().accumulate(Result.success<string, int>(1)).accumulate(Result.failure<string, int>("Error occurred")).accumulate(Result.success<string, int>(3)).toResult().getFailure());
    [Fact] public void shouldStopAccumulatingToSetOnFirstFailure() => Assert.Equal("Error occurred", Result.compositeSet<string, int>().accumulate(Result.success<string, int>(1)).accumulate(Result.failure<string, int>("Error occurred")).accumulate(Result.success<string, int>(3)).toResult().getFailure());
    [Fact] public void shouldRetainFailureWhenAccumulatingToFailedComposite() => Assert.Equal("First error", Result.composite<string, int>().accumulate(Result.failure<string, int>("First error")).accumulate(Result.success<string, int>(1)).toResult().getFailure());
    [Fact] public void shouldRetainFailureWhenAccumulatingToSetToFailedComposite() => Assert.Equal("First error", Result.compositeSet<string, int>().accumulate(Result.failure<string, int>("First error")).accumulate(Result.success<string, int>(1)).toResult().getFailure());

    [Fact]
    public void shouldAccumulateToSetRemovingDuplicates() =>
        Assert.True(Result.compositeSet<string, int>().accumulate(Result.success<string, int>(1)).accumulate(Result.success<string, int>(2)).accumulate(Result.success<string, int>(1)).accumulate(Result.success<string, int>(3)).toResult().getSuccess().SetEquals([1, 2, 3]));

    [Fact] public void shouldThrowIllegalArgumentExceptionWhenAccumulateWithNull() => Assert.Throws<ArgumentException>(() => Result.composite<string, int>().accumulate(null));
    [Fact] public void shouldThrowIllegalArgumentExceptionWhenAccumulateToSetWithNull() => Assert.Throws<ArgumentException>(() => Result.compositeSet<string, int>().accumulate(null));

    [Fact]
    public void shouldReturnTrueForSuccessOnCompositeResult()
    {
        Result.CompositeResult<string, int> composite = Result.composite<string, int>().accumulate(Result.success<string, int>(1));
        Assert.True(composite.success());
        Assert.False(composite.failure());
    }

    [Fact]
    public void shouldReturnTrueForFailureOnCompositeResult()
    {
        Result.CompositeResult<string, int> composite = Result.composite<string, int>().accumulate(Result.failure<string, int>("Error"));
        Assert.True(composite.failure());
        Assert.False(composite.success());
    }

    [Fact]
    public void shouldReturnTrueForSuccessOnCompositeSetResult()
    {
        Result.CompositeSetResult<string, int> composite = Result.compositeSet<string, int>().accumulate(Result.success<string, int>(1));
        Assert.True(composite.success());
        Assert.False(composite.failure());
    }

    [Fact]
    public void shouldReturnTrueForFailureOnCompositeSetResult()
    {
        Result.CompositeSetResult<string, int> composite = Result.compositeSet<string, int>().accumulate(Result.failure<string, int>("Error"));
        Assert.True(composite.failure());
        Assert.False(composite.success());
    }
}
