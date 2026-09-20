using System;
using System.Collections.Generic;

namespace SoftwareArchetypes.Common;

public abstract class Result<F, S>
{
    private protected Result()
    {
    }

    public Result<L, R> BiMap<L, R>(Func<S, R>? successMapper, Func<F, L>? failureMapper)
    {
        Preconditions.CheckNotNull(successMapper, "successMapper cannot be null");
        Preconditions.CheckNotNull(failureMapper, "failureMapper cannot be null");
        return IsSuccess()
            ? Result.CreateSuccess<L, R>(successMapper(GetSuccess()))
            : Result.CreateFailure<L, R>(failureMapper(GetFailure()));
    }

    public Result<F, R> Map<R>(Func<S, R>? mapper)
    {
        Preconditions.CheckNotNull(mapper, "mapper cannot be null");
        return IsSuccess()
            ? Result.CreateSuccess<F, R>(mapper(GetSuccess()))
            : Result.CreateFailure<F, R>(GetFailure());
    }

    public Result<L, S> MapFailure<L>(Func<F, L>? mapper)
    {
        Preconditions.CheckNotNull(mapper, "mapper cannot be null");
        return IsSuccess()
            ? Result.CreateSuccess<L, S>(GetSuccess())
            : Result.CreateFailure<L, S>(mapper(GetFailure()));
    }

    public Result<F, S> Peek(Action<S>? successConsumer, Action<F>? failureConsumer)
    {
        Preconditions.CheckNotNull(successConsumer, "successConsumer cannot be null");
        Preconditions.CheckNotNull(failureConsumer, "failureConsumer cannot be null");
        if (IsSuccess())
        {
            successConsumer(GetSuccess());
        }
        else
        {
            failureConsumer(GetFailure());
        }

        return this;
    }

    public Result<F, S> PeekSuccess(Action<S>? successConsumer)
    {
        Preconditions.CheckNotNull(successConsumer, "successConsumer cannot be null");
        return Peek(successConsumer, _ => { });
    }

    public Result<F, S> PeekFailure(Action<F>? failureConsumer)
    {
        Preconditions.CheckNotNull(failureConsumer, "failureConsumer cannot be null");
        return Peek(_ => { }, failureConsumer);
    }

    public R IfSuccessOrElse<R>(Func<S, R>? successMapping, Func<F, R>? failureMapping)
    {
        Preconditions.CheckNotNull(successMapping, "successMapping cannot be null");
        Preconditions.CheckNotNull(failureMapping, "failureMapping cannot be null");
        return IsSuccess() ? successMapping(GetSuccess()) : failureMapping(GetFailure());
    }

    public Result<F, R> FlatMap<R>(Func<S, Result<F, R>>? mapping)
    {
        Preconditions.CheckNotNull(mapping, "mapping cannot be null");
        return IsSuccess() ? mapping(GetSuccess()) : Result.CreateFailure<F, R>(GetFailure());
    }

    public U Fold<U>(Func<F, U>? leftMapper, Func<S, U>? rightMapper)
    {
        Preconditions.CheckNotNull(leftMapper, "leftMapper cannot be null");
        Preconditions.CheckNotNull(rightMapper, "rightMapper cannot be null");
        return IsSuccess() ? rightMapper(GetSuccess()) : leftMapper(GetFailure());
    }

    public Result<FAILURE, SUCCESS> Combine<FAILURE, SUCCESS>(
        Result<F, S>? secondResult,
        Func<F?, F?, FAILURE>? failureCombiner,
        Func<S, S, SUCCESS>? successCombiner)
    {
        Preconditions.CheckNotNull(secondResult, "secondResult cannot be null");
        Preconditions.CheckNotNull(failureCombiner, "failureCombiner cannot be null");
        Preconditions.CheckNotNull(successCombiner, "successCombiner cannot be null");
        if (IsSuccess() && secondResult.IsSuccess())
        {
            return Result.CreateSuccess<FAILURE, SUCCESS>(successCombiner(GetSuccess(), secondResult.GetSuccess()));
        }

        F? firstFailure = IsFailure() ? GetFailure() : default;
        F? secondFailure = secondResult.IsFailure() ? secondResult.GetFailure() : default;
        return Result.CreateFailure<FAILURE, SUCCESS>(failureCombiner(firstFailure, secondFailure));
    }

    public abstract bool IsSuccess();

    public abstract bool IsFailure();

    public virtual S GetSuccess() => throw new InvalidOperationException();

    public virtual F GetFailure() => throw new InvalidOperationException();
}

public static class Result
{
    public static Result<F, S> CreateSuccess<F, S>(S value) => new Success<F, S>(value);

    public static Result<F, S> CreateFailure<F, S>(F value) => new Failure<F, S>(value);

    public static CompositeResult<F, S> Composite<F, S>() => new([]);

    public static CompositeSetResult<F, S> CompositeSet<F, S>() => new([]);

    public sealed class Success<F, S> : Result<F, S>
    {
        private readonly S value;

        internal Success(S value)
        {
            this.value = value;
        }

        public override bool IsSuccess() => true;

        public override bool IsFailure() => false;

        public override S GetSuccess() => value;
    }

    public sealed class Failure<F, S> : Result<F, S>
    {
        private readonly F value;

        internal Failure(F value)
        {
            this.value = value;
        }

        public override bool IsSuccess() => false;

        public override bool IsFailure() => true;

        public override F GetFailure() => value;
    }

    public sealed class CompositeResult<F, S>
    {
        private readonly Result<F, IReadOnlyList<S>> result;

        internal CompositeResult(List<S> initialValues)
        {
            result = CreateSuccess<F, IReadOnlyList<S>>(initialValues);
        }

        private CompositeResult(F failureValue)
        {
            result = CreateFailure<F, IReadOnlyList<S>>(failureValue);
        }

        public CompositeResult<F, S> Accumulate(Result<F, S>? newResult)
        {
            Preconditions.CheckNotNull(newResult, "newResult cannot be null");
            if (result.IsFailure())
            {
                return this;
            }

            if (newResult.IsFailure())
            {
                return new CompositeResult<F, S>(newResult.GetFailure());
            }

            List<S> accumulated = new(result.GetSuccess()) { newResult.GetSuccess() };
            return new CompositeResult<F, S>(accumulated);
        }

        public bool IsSuccess() => result.IsSuccess();

        public bool IsFailure() => result.IsFailure();

        public Result<F, IReadOnlyList<S>> ToResult() => result;
    }

    public sealed class CompositeSetResult<F, S>
    {
        private readonly Result<F, IReadOnlySet<S>> result;

        internal CompositeSetResult(HashSet<S> initialValues)
        {
            result = CreateSuccess<F, IReadOnlySet<S>>(initialValues);
        }

        private CompositeSetResult(F failureValue)
        {
            result = CreateFailure<F, IReadOnlySet<S>>(failureValue);
        }

        public CompositeSetResult<F, S> Accumulate(Result<F, S>? newResult)
        {
            Preconditions.CheckNotNull(newResult, "newResult cannot be null");
            if (result.IsFailure())
            {
                return this;
            }

            if (newResult.IsFailure())
            {
                return new CompositeSetResult<F, S>(newResult.GetFailure());
            }

            HashSet<S> accumulated = new(result.GetSuccess()) { newResult.GetSuccess() };
            return new CompositeSetResult<F, S>(accumulated);
        }

        public bool IsSuccess() => result.IsSuccess();

        public bool IsFailure() => result.IsFailure();

        public Result<F, IReadOnlySet<S>> ToResult() => result;
    }
}
