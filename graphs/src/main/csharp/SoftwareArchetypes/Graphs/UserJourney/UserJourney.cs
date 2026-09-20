using System;
using System.Collections.Generic;

namespace SoftwareArchetypes.Graphs.UserJourney;

internal sealed class UserJourney : IEquatable<UserJourney>
{
    private readonly UserJourneyId userJourneyIdValue;
    private readonly JourneyGraph graphValue;
    private readonly State currentStateValue;

    internal UserJourney(UserJourneyId userJourneyId, JourneyGraph graph, State currentState)
    {
        userJourneyIdValue = userJourneyId;
        graphValue = graph;
        currentStateValue = currentState;
    }

    internal UserJourneyId UserJourneyId() => userJourneyIdValue;

    internal JourneyGraph Graph() => graphValue;

    internal State CurrentState() => currentStateValue;

    internal static Builder Builder(UserJourneyId userJourneyId) => new(userJourneyId);

    internal ISet<CustomerPath> WaysToAchieve(Product.ProductType productType)
    {
        HashSet<CustomerPath> paths = [];
        foreach (State target in graphValue.Vertices())
        {
            if (target.Contains(productType))
            {
                FindPaths(currentStateValue, target, [], [], paths);
            }
        }

        return paths;
    }

    internal UserJourney OnFulfilled(Condition condition)
    {
        foreach (JourneyEdge edge in graphValue.OutgoingEdgesOf(currentStateValue))
        {
            if (edge.condition.Equals(condition))
            {
                return new UserJourney(userJourneyIdValue, graphValue, edge.to);
            }
        }

        return this;
    }

    internal CustomerPath? OptimizedWayToAchieve(
        Product.ProductType productType,
        Func<Condition, double> weightFunction)
    {
        CustomerPath? best = null;
        double bestWeight = double.PositiveInfinity;
        foreach (CustomerPath path in WaysToAchieve(productType))
        {
            double weight = path.Weight(weightFunction);
            if (weight < bestWeight)
            {
                best = path;
                bestWeight = weight;
            }
        }

        return best;
    }

    public bool Equals(UserJourney? other) =>
        other is not null &&
        userJourneyIdValue.Equals(other.userJourneyIdValue) &&
        ReferenceEquals(graphValue, other.graphValue) &&
        currentStateValue.Equals(other.currentStateValue);

    public override bool Equals(object? obj) => obj is UserJourney other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(userJourneyIdValue, graphValue, currentStateValue);

    private void FindPaths(
        State current,
        State target,
        HashSet<State> visited,
        List<Condition> conditions,
        ISet<CustomerPath> result)
    {
        if (current.Equals(target))
        {
            result.Add(CustomerPath.Of(conditions));
            return;
        }

        visited.Add(current);
        foreach (JourneyEdge edge in graphValue.OutgoingEdgesOf(current))
        {
            if (!visited.Contains(edge.to))
            {
                conditions.Add(edge.condition);
                FindPaths(edge.to, target, visited, conditions, result);
                conditions.RemoveAt(conditions.Count - 1);
            }
        }

        visited.Remove(current);
    }
}

internal sealed class Builder
{
    internal readonly UserJourneyId userJourneyId;
    internal readonly JourneyGraph graph = new();
    internal State? currentState;

    internal Builder(UserJourneyId userJourneyId)
    {
        this.userJourneyId = userJourneyId;
    }

    internal TransitionBuilder From(State state)
    {
        graph.AddVertex(state);
        return new TransitionBuilder(this, state);
    }

    internal Builder WithCurrentState(State value)
    {
        currentState = value;
        return this;
    }

    internal UserJourney Build() =>
        new(userJourneyId, graph, currentState ?? throw new NullReferenceException());
}

internal sealed class TransitionBuilder
{
    private readonly Builder builder;
    private readonly State fromState;

    internal TransitionBuilder(Builder builder, State fromState)
    {
        this.builder = builder;
        this.fromState = fromState;
    }

    internal TransitionWithCondition On(Condition condition) => new(builder, fromState, condition);
}

internal sealed class TransitionWithCondition
{
    private readonly Builder builder;
    private readonly State fromState;
    private readonly Condition condition;

    internal TransitionWithCondition(Builder builder, State fromState, Condition condition)
    {
        this.builder = builder;
        this.fromState = fromState;
        this.condition = condition;
    }

    internal Builder Goto_(State toState)
    {
        builder.graph.AddVertex(toState);
        builder.graph.AddEdge(fromState, toState, condition);
        return builder;
    }
}

internal sealed record class JourneyEdge(State from, State to, Condition condition);

internal sealed class JourneyGraph
{
    private readonly HashSet<State> vertexSet = [];
    private readonly List<State> vertexOrder = [];
    private readonly Dictionary<State, List<JourneyEdge>> outgoing = [];
    private readonly HashSet<Condition> usedConditions = [];

    internal IReadOnlyList<State> Vertices() => vertexOrder;

    internal void AddVertex(State state)
    {
        if (vertexSet.Add(state))
        {
            vertexOrder.Add(state);
            outgoing.Add(state, []);
        }
    }

    internal void AddEdge(State from, State to, Condition condition)
    {
        if (!usedConditions.Add(condition))
        {
            throw new ArgumentException("Edge already associated with source and target vertices");
        }

        outgoing[from].Add(new JourneyEdge(from, to, condition));
    }

    internal IReadOnlyList<JourneyEdge> OutgoingEdgesOf(State state) => outgoing[state];
}
