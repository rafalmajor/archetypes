using System;
using System.Collections.Generic;
using Xunit;

namespace SoftwareArchetypes.Common;

public sealed class ResultTest
{
    [Fact] public void ShouldBeMarkedAsSuccessForSuccessResult() => Assert.True(Result.CreateSuccess<string, string>("value").IsSuccess());
    [Fact] public void ShouldNotBeMarkedAsFailureForSuccessResult() => Assert.False(Result.CreateSuccess<string, string>("value").IsFailure());
    [Fact] public void ShouldBeMarkedAsFailureForFailureResult() => Assert.True(Result.CreateFailure<string, string>("error").IsFailure());
    [Fact] public void ShouldNotBeMarkedAsSuccessForFailureResult() => Assert.False(Result.CreateFailure<string, string>("error").IsSuccess());
    [Fact] public void ShouldFailToGetSuccessOnFailureResult() => Assert.Throws<InvalidOperationException>(() => Result.CreateFailure<string, string>("error").GetSuccess());
    [Fact] public void ShouldFailToGetFailureOnSuccessResult() => Assert.Throws<InvalidOperationException>(() => Result.CreateSuccess<string, string>("value").GetFailure());

    [Fact]
    public void ShouldChooseAndProperlyApplySuccessMappingFunction() =>
        Assert.Equal("SUCCESS-value", Result.CreateSuccess<string, string>("value").IfSuccessOrElse(value => "SUCCESS-" + value, value => "FAILURE-" + value));

    [Fact]
    public void ShouldChooseAndProperlyApplyFailureMappingFunction() =>
        Assert.Equal("FAILURE-error", Result.CreateFailure<string, string>("error").IfSuccessOrElse(value => "SUCCESS-" + value, value => "FAILURE-" + value));

    [Fact]
    public void ShouldMapSuccessResultAccordingToMappingFunction() =>
        Assert.Equal("2", Result.CreateSuccess<int, int>(1).BiMap(value => (value * 2).ToString(), _ => "").GetSuccess());

    [Fact]
    public void ShouldMapFailureResultAccordingToMappingFunction() =>
        Assert.Equal("", Result.CreateFailure<int, int>(1).BiMap(value => (value * 2).ToString(), _ => "").GetFailure());

    [Fact]
    public void ShouldCallSuccessConsumerOnPeek()
    {
        int successCalls = 0;
        int failureCalls = 0;
        Result<int, int> result = Result.CreateSuccess<int, int>(7);
        Result<int, int> peekResult = result.Peek(_ => successCalls++, _ => failureCalls++);
        Assert.Same(result, peekResult);
        Assert.Equal(1, successCalls);
        Assert.Equal(0, failureCalls);
    }

    [Fact]
    public void ShouldCallSuccessConsumerOnPeekSuccess()
    {
        int successCalls = 0;
        int failureCalls = 0;
        Result<int, int> result = Result.CreateSuccess<int, int>(7);
        Result<int, int> peekResult = result.PeekSuccess(_ => successCalls++).PeekFailure(_ => failureCalls++);
        Assert.Same(result, peekResult);
        Assert.Equal(1, successCalls);
        Assert.Equal(0, failureCalls);
    }

    [Fact]
    public void ShouldCallFailureConsumerOnPeek()
    {
        int successCalls = 0;
        int failureCalls = 0;
        Result<int, int> result = Result.CreateFailure<int, int>(7);
        Result<int, int> peekResult = result.Peek(_ => successCalls++, _ => failureCalls++);
        Assert.Same(result, peekResult);
        Assert.Equal(0, successCalls);
        Assert.Equal(1, failureCalls);
    }

    [Fact]
    public void ShouldCallFailureConsumerOnPeekFailure()
    {
        int successCalls = 0;
        int failureCalls = 0;
        Result<int, int> result = Result.CreateFailure<int, int>(7);
        Result<int, int> peekResult = result.PeekSuccess(_ => successCalls++).PeekFailure(_ => failureCalls++);
        Assert.Same(result, peekResult);
        Assert.Equal(0, successCalls);
        Assert.Equal(1, failureCalls);
    }

    [Fact]
    public void ShouldCombineTwoSuccessResults()
    {
        Result<int?, int> result = Result.CreateSuccess<int?, int>(5).Combine(Result.CreateSuccess<int?, int>(3), (left, right) => left - right, (left, right) => left + right);
        Assert.Equal(8, result.GetSuccess());
    }

    [Fact]
    public void ShouldCombineTwoFailureResults()
    {
        Result<int?, int> result = Result.CreateFailure<int?, int>(5).Combine(Result.CreateFailure<int?, int>(3), (left, right) => left - right, (left, right) => left + right);
        Assert.Equal(2, result.GetFailure());
    }

    [Fact]
    public void ShouldProduceFailureResultWhenCombiningFailureAndSuccessResults()
    {
        Result<int?, int> success = Result.CreateSuccess<int?, int>(5);
        Result<int?, int> failure = Result.CreateFailure<int?, int>(3);
        Func<int?, int?, int?> combiner = (left, right) => (left ?? 0) - (right ?? 0);
        Assert.Equal(-3, success.Combine(failure, combiner, (left, right) => left + right).GetFailure());
        Assert.Equal(3, failure.Combine(success, combiner, (left, right) => left + right).GetFailure());
    }

    [Fact] public void ShouldMapSuccessValueUsingMapFunction() => Assert.Equal("Value: 20", Result.CreateSuccess<string, int>(10).Map(value => $"Value: {value * 2}").GetSuccess());
    [Fact] public void ShouldNotMapFailureValueUsingMapFunction() => Assert.Equal("Error occurred", Result.CreateFailure<string, int>("Error occurred").Map(value => $"Value: {value * 2}").GetFailure());
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenMapWithNullMapper() => Assert.Throws<ArgumentException>(() => Result.CreateSuccess<string, int>(10).Map<string>(null));
    [Fact] public void ShouldMapFailureValueUsingMapFailureFunction() => Assert.Equal("Error 404: Not Found", Result.CreateFailure<int, int>(404).MapFailure(code => $"Error {code}: Not Found").GetFailure());
    [Fact] public void ShouldNotMapSuccessValueUsingMapFailureFunction() => Assert.Equal(42, Result.CreateSuccess<int, int>(42).MapFailure(code => $"Error {code}").GetSuccess());
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenMapFailureWithNullMapper() => Assert.Throws<ArgumentException>(() => Result.CreateFailure<string, int>("error").MapFailure<int>(null));
    [Fact] public void ShouldFlatMapSuccessResultWithAnotherSuccessResult() => Assert.Equal(10, Result.CreateSuccess<string, int>(5).FlatMap(value => Result.CreateSuccess<string, int>(value * 2)).GetSuccess());
    [Fact] public void ShouldFlatMapSuccessResultWithFailureResult() => Assert.Equal("Validation failed", Result.CreateSuccess<string, int>(5).FlatMap(_ => Result.CreateFailure<string, int>("Validation failed")).GetFailure());
    [Fact] public void ShouldNotFlatMapFailureResult() => Assert.Equal("Initial error", Result.CreateFailure<string, int>("Initial error").FlatMap(value => Result.CreateSuccess<string, int>(value * 2)).GetFailure());
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenFlatMapWithNullMapper() => Assert.Throws<ArgumentException>(() => Result.CreateSuccess<string, int>(10).FlatMap<string>(null));
    [Fact] public void ShouldFoldSuccessResultUsingRightMapper() => Assert.Equal(30, Result.CreateSuccess<string, int>(10).Fold(_ => -1, value => value * 3));
    [Fact] public void ShouldFoldFailureResultUsingLeftMapper() => Assert.Equal(5, Result.CreateFailure<string, int>("Error").Fold(error => error.Length, value => value * 3));
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenFoldingWithNullLeftMapper() => Assert.Throws<ArgumentException>(() => Result.CreateSuccess<string, int>(10).Fold<int>(null, value => value * 2));
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenFoldingWithNullRightMapper() => Assert.Throws<ArgumentException>(() => Result.CreateSuccess<string, int>(10).Fold<int>(_ => -1, null));
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenBiMapWithNullSuccessMapper() => Assert.Throws<ArgumentException>(() => Result.CreateSuccess<string, int>(10).BiMap<string, string>(null, _ => ""));
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenBiMapWithNullFailureMapper() => Assert.Throws<ArgumentException>(() => Result.CreateSuccess<string, int>(10).BiMap<string, string>(_ => "", null));
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenIfSuccessOrElseWithNullSuccessMapping() => Assert.Throws<ArgumentException>(() => Result.CreateSuccess<string, int>(10).IfSuccessOrElse<string>(null, _ => ""));
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenIfSuccessOrElseWithNullFailureMapping() => Assert.Throws<ArgumentException>(() => Result.CreateSuccess<string, int>(10).IfSuccessOrElse<string>(_ => "", null));
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenPeekWithNullSuccessConsumer() => Assert.Throws<ArgumentException>(() => Result.CreateSuccess<string, int>(10).Peek(null, _ => { }));
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenPeekWithNullFailureConsumer() => Assert.Throws<ArgumentException>(() => Result.CreateSuccess<string, int>(10).Peek(_ => { }, null));
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenPeekSuccessWithNullConsumer() => Assert.Throws<ArgumentException>(() => Result.CreateSuccess<string, int>(10).PeekSuccess(null));
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenPeekFailureWithNullConsumer() => Assert.Throws<ArgumentException>(() => Result.CreateFailure<string, int>("error").PeekFailure(null));
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenCombineWithNullSecondResult() => Assert.Throws<ArgumentException>(() => Result.CreateSuccess<string, int>(10).Combine<string, int>(null, (_, _) => "", (left, right) => left + right));
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenCombineWithNullFailureCombiner() => Assert.Throws<ArgumentException>(() => Result.CreateSuccess<string, int>(10).Combine<string, int>(Result.CreateSuccess<string, int>(20), null, (left, right) => left + right));
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenCombineWithNullSuccessCombiner() => Assert.Throws<ArgumentException>(() => Result.CreateSuccess<string, int>(10).Combine<string, int>(Result.CreateSuccess<string, int>(20), (_, _) => "", null));

    [Fact] public void ShouldCreateEmptyCompositeResult() => Assert.Empty(Result.Composite<string, int>().ToResult().GetSuccess());
    [Fact] public void ShouldCreateEmptyCompositeSetResult() => Assert.Empty(Result.CompositeSet<string, int>().ToResult().GetSuccess());

    [Fact]
    public void ShouldAccumulateSuccessResultsIntoList() =>
        Assert.Equal([1, 2, 3], Result.Composite<string, int>().Accumulate(Result.CreateSuccess<string, int>(1)).Accumulate(Result.CreateSuccess<string, int>(2)).Accumulate(Result.CreateSuccess<string, int>(3)).ToResult().GetSuccess());

    [Fact]
    public void ShouldAccumulateSuccessResultsIntoSet() =>
        Assert.True(Result.CompositeSet<string, int>().Accumulate(Result.CreateSuccess<string, int>(1)).Accumulate(Result.CreateSuccess<string, int>(2)).Accumulate(Result.CreateSuccess<string, int>(3)).ToResult().GetSuccess().SetEquals([1, 2, 3]));

    [Fact] public void ShouldStopAccumulatingOnFirstFailure() => Assert.Equal("Error occurred", Result.Composite<string, int>().Accumulate(Result.CreateSuccess<string, int>(1)).Accumulate(Result.CreateFailure<string, int>("Error occurred")).Accumulate(Result.CreateSuccess<string, int>(3)).ToResult().GetFailure());
    [Fact] public void ShouldStopAccumulatingToSetOnFirstFailure() => Assert.Equal("Error occurred", Result.CompositeSet<string, int>().Accumulate(Result.CreateSuccess<string, int>(1)).Accumulate(Result.CreateFailure<string, int>("Error occurred")).Accumulate(Result.CreateSuccess<string, int>(3)).ToResult().GetFailure());
    [Fact] public void ShouldRetainFailureWhenAccumulatingToFailedComposite() => Assert.Equal("First error", Result.Composite<string, int>().Accumulate(Result.CreateFailure<string, int>("First error")).Accumulate(Result.CreateSuccess<string, int>(1)).ToResult().GetFailure());
    [Fact] public void ShouldRetainFailureWhenAccumulatingToSetToFailedComposite() => Assert.Equal("First error", Result.CompositeSet<string, int>().Accumulate(Result.CreateFailure<string, int>("First error")).Accumulate(Result.CreateSuccess<string, int>(1)).ToResult().GetFailure());

    [Fact]
    public void ShouldAccumulateToSetRemovingDuplicates() =>
        Assert.True(Result.CompositeSet<string, int>().Accumulate(Result.CreateSuccess<string, int>(1)).Accumulate(Result.CreateSuccess<string, int>(2)).Accumulate(Result.CreateSuccess<string, int>(1)).Accumulate(Result.CreateSuccess<string, int>(3)).ToResult().GetSuccess().SetEquals([1, 2, 3]));

    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenAccumulateWithNull() => Assert.Throws<ArgumentException>(() => Result.Composite<string, int>().Accumulate(null));
    [Fact] public void ShouldThrowIllegalArgumentExceptionWhenAccumulateToSetWithNull() => Assert.Throws<ArgumentException>(() => Result.CompositeSet<string, int>().Accumulate(null));

    [Fact]
    public void ShouldReturnTrueForSuccessOnCompositeResult()
    {
        Result.CompositeResult<string, int> composite = Result.Composite<string, int>().Accumulate(Result.CreateSuccess<string, int>(1));
        Assert.True(composite.IsSuccess());
        Assert.False(composite.IsFailure());
    }

    [Fact]
    public void ShouldReturnTrueForFailureOnCompositeResult()
    {
        Result.CompositeResult<string, int> composite = Result.Composite<string, int>().Accumulate(Result.CreateFailure<string, int>("Error"));
        Assert.True(composite.IsFailure());
        Assert.False(composite.IsSuccess());
    }

    [Fact]
    public void ShouldReturnTrueForSuccessOnCompositeSetResult()
    {
        Result.CompositeSetResult<string, int> composite = Result.CompositeSet<string, int>().Accumulate(Result.CreateSuccess<string, int>(1));
        Assert.True(composite.IsSuccess());
        Assert.False(composite.IsFailure());
    }

    [Fact]
    public void ShouldReturnTrueForFailureOnCompositeSetResult()
    {
        Result.CompositeSetResult<string, int> composite = Result.CompositeSet<string, int>().Accumulate(Result.CreateFailure<string, int>("Error"));
        Assert.True(composite.IsFailure());
        Assert.False(composite.IsSuccess());
    }
}
