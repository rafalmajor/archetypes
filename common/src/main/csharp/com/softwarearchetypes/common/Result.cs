using System;
using System.Collections.Generic;

namespace com.softwarearchetypes.common;

public abstract class Result<F, S>
{
    private protected Result()
    {
    }

    public Result<L, R> biMap<L, R>(Func<S, R>? successMapper, Func<F, L>? failureMapper)
    {
        Preconditions.checkNotNull(successMapper, "successMapper cannot be null");
        Preconditions.checkNotNull(failureMapper, "failureMapper cannot be null");
        return success()
            ? Result.success<L, R>(successMapper(getSuccess()))
            : Result.failure<L, R>(failureMapper(getFailure()));
    }

    public Result<F, R> map<R>(Func<S, R>? mapper)
    {
        Preconditions.checkNotNull(mapper, "mapper cannot be null");
        return success()
            ? Result.success<F, R>(mapper(getSuccess()))
            : Result.failure<F, R>(getFailure());
    }

    public Result<L, S> mapFailure<L>(Func<F, L>? mapper)
    {
        Preconditions.checkNotNull(mapper, "mapper cannot be null");
        return success()
            ? Result.success<L, S>(getSuccess())
            : Result.failure<L, S>(mapper(getFailure()));
    }

    public Result<F, S> peek(Action<S>? successConsumer, Action<F>? failureConsumer)
    {
        Preconditions.checkNotNull(successConsumer, "successConsumer cannot be null");
        Preconditions.checkNotNull(failureConsumer, "failureConsumer cannot be null");
        if (success())
        {
            successConsumer(getSuccess());
        }
        else
        {
            failureConsumer(getFailure());
        }

        return this;
    }

    public Result<F, S> peekSuccess(Action<S>? successConsumer)
    {
        Preconditions.checkNotNull(successConsumer, "successConsumer cannot be null");
        return peek(successConsumer, _ => { });
    }

    public Result<F, S> peekFailure(Action<F>? failureConsumer)
    {
        Preconditions.checkNotNull(failureConsumer, "failureConsumer cannot be null");
        return peek(_ => { }, failureConsumer);
    }

    public R ifSuccessOrElse<R>(Func<S, R>? successMapping, Func<F, R>? failureMapping)
    {
        Preconditions.checkNotNull(successMapping, "successMapping cannot be null");
        Preconditions.checkNotNull(failureMapping, "failureMapping cannot be null");
        return success() ? successMapping(getSuccess()) : failureMapping(getFailure());
    }

    public Result<F, R> flatMap<R>(Func<S, Result<F, R>>? mapping)
    {
        Preconditions.checkNotNull(mapping, "mapping cannot be null");
        return success() ? mapping(getSuccess()) : Result.failure<F, R>(getFailure());
    }

    public U fold<U>(Func<F, U>? leftMapper, Func<S, U>? rightMapper)
    {
        Preconditions.checkNotNull(leftMapper, "leftMapper cannot be null");
        Preconditions.checkNotNull(rightMapper, "rightMapper cannot be null");
        return success() ? rightMapper(getSuccess()) : leftMapper(getFailure());
    }

    public Result<FAILURE, SUCCESS> combine<FAILURE, SUCCESS>(
        Result<F, S>? secondResult,
        Func<F?, F?, FAILURE>? failureCombiner,
        Func<S, S, SUCCESS>? successCombiner)
    {
        Preconditions.checkNotNull(secondResult, "secondResult cannot be null");
        Preconditions.checkNotNull(failureCombiner, "failureCombiner cannot be null");
        Preconditions.checkNotNull(successCombiner, "successCombiner cannot be null");
        if (success() && secondResult.success())
        {
            return Result.success<FAILURE, SUCCESS>(successCombiner(getSuccess(), secondResult.getSuccess()));
        }

        F? firstFailure = failure() ? getFailure() : default;
        F? secondFailure = secondResult.failure() ? secondResult.getFailure() : default;
        return Result.failure<FAILURE, SUCCESS>(failureCombiner(firstFailure, secondFailure));
    }

    public abstract bool success();

    public abstract bool failure();

    public virtual S getSuccess() => throw new InvalidOperationException();

    public virtual F getFailure() => throw new InvalidOperationException();
}

public static class Result
{
    public static Result<F, S> success<F, S>(S value) => new Success<F, S>(value);

    public static Result<F, S> failure<F, S>(F value) => new Failure<F, S>(value);

    public static CompositeResult<F, S> composite<F, S>() => new([]);

    public static CompositeSetResult<F, S> compositeSet<F, S>() => new([]);

    public sealed class Success<F, S> : Result<F, S>
    {
        private readonly S value;

        internal Success(S value)
        {
            this.value = value;
        }

        public override bool success() => true;

        public override bool failure() => false;

        public override S getSuccess() => value;
    }

    public sealed class Failure<F, S> : Result<F, S>
    {
        private readonly F value;

        internal Failure(F value)
        {
            this.value = value;
        }

        public override bool success() => false;

        public override bool failure() => true;

        public override F getFailure() => value;
    }

    public sealed class CompositeResult<F, S>
    {
        private readonly Result<F, IReadOnlyList<S>> result;

        internal CompositeResult(List<S> initialValues)
        {
            result = success<F, IReadOnlyList<S>>(initialValues);
        }

        private CompositeResult(F failureValue)
        {
            result = failure<F, IReadOnlyList<S>>(failureValue);
        }

        public CompositeResult<F, S> accumulate(Result<F, S>? newResult)
        {
            Preconditions.checkNotNull(newResult, "newResult cannot be null");
            if (result.failure())
            {
                return this;
            }

            if (newResult.failure())
            {
                return new CompositeResult<F, S>(newResult.getFailure());
            }

            List<S> accumulated = new(result.getSuccess()) { newResult.getSuccess() };
            return new CompositeResult<F, S>(accumulated);
        }

        public bool success() => result.success();

        public bool failure() => result.failure();

        public Result<F, IReadOnlyList<S>> toResult() => result;
    }

    public sealed class CompositeSetResult<F, S>
    {
        private readonly Result<F, IReadOnlySet<S>> result;

        internal CompositeSetResult(HashSet<S> initialValues)
        {
            result = success<F, IReadOnlySet<S>>(initialValues);
        }

        private CompositeSetResult(F failureValue)
        {
            result = failure<F, IReadOnlySet<S>>(failureValue);
        }

        public CompositeSetResult<F, S> accumulate(Result<F, S>? newResult)
        {
            Preconditions.checkNotNull(newResult, "newResult cannot be null");
            if (result.failure())
            {
                return this;
            }

            if (newResult.failure())
            {
                return new CompositeSetResult<F, S>(newResult.getFailure());
            }

            HashSet<S> accumulated = new(result.getSuccess()) { newResult.getSuccess() };
            return new CompositeSetResult<F, S>(accumulated);
        }

        public bool success() => result.success();

        public bool failure() => result.failure();

        public Result<F, IReadOnlySet<S>> toResult() => result;
    }
}
