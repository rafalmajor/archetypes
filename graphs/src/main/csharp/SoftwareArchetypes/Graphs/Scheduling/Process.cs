using System;
using System.Collections.Generic;

namespace SoftwareArchetypes.Graphs.Scheduling;

internal sealed class Process
{
    private readonly IReadOnlySet<ProcessStep> stepsValue;
    private readonly IReadOnlyDictionary<ProcessStep, IReadOnlyList<ProcessStep>> dependencyGraphValue;
    private readonly IReadOnlyDictionary<EdgeKey, DependencyType> edgeDependencyTypesValue;

    private Process(
        IReadOnlySet<ProcessStep> steps,
        IReadOnlyDictionary<ProcessStep, IReadOnlyList<ProcessStep>> dependencyGraph,
        IReadOnlyDictionary<EdgeKey, DependencyType> edgeDependencyTypes)
    {
        stepsValue = steps;
        dependencyGraphValue = dependencyGraph;
        edgeDependencyTypesValue = edgeDependencyTypes;
    }

    internal IReadOnlySet<ProcessStep> Steps() => stepsValue;

    internal IReadOnlyDictionary<ProcessStep, IReadOnlyList<ProcessStep>> DependencyGraph() => dependencyGraphValue;

    internal IReadOnlyDictionary<EdgeKey, DependencyType> EdgeDependencyTypes() => edgeDependencyTypesValue;

    internal static Builder CreateBuilder() => new();

    internal sealed class Builder
    {
        private readonly HashSet<ProcessStep> steps = [];
        private readonly List<ProcessStep> insertionOrder = [];
        private readonly Dictionary<ProcessStep, List<ProcessStep>> graph = [];
        private readonly Dictionary<EdgeKey, DependencyType> edgeDependencyTypes = [];

        internal Builder AddStep(ProcessStep step)
        {
            if (steps.Add(step))
            {
                insertionOrder.Add(step);
                graph.Add(step, []);
            }

            return this;
        }

        internal Builder AddDependency(ProcessStep from, ProcessStep to) => AddDependencyCore(from, to, null);

        internal Builder AddDependency(ProcessStep from, ProcessStep to, DependencyType dependencyType) =>
            AddDependencyCore(from, to, dependencyType);

        internal Schedule Build()
        {
            Dictionary<ProcessStep, IReadOnlyList<ProcessStep>> graphCopy = [];
            foreach ((ProcessStep step, List<ProcessStep> successors) in graph)
            {
                graphCopy.Add(step, successors.AsReadOnly());
            }

            Process process = new(
                new HashSet<ProcessStep>(steps),
                graphCopy,
                new Dictionary<EdgeKey, DependencyType>(edgeDependencyTypes));
            return CalculateSchedule(process);
        }

        private Builder AddDependencyCore(ProcessStep from, ProcessStep to, DependencyType? dependencyType)
        {
            AddStep(from);
            AddStep(to);
            if (from.Equals(to) || HasPath(to, from))
            {
                throw new ArgumentException("Edge would induce a cycle");
            }

            if (!graph[from].Contains(to))
            {
                graph[from].Add(to);
            }

            if (dependencyType is not null)
            {
                edgeDependencyTypes[new EdgeKey(from, to)] = dependencyType;
            }

            return this;
        }

        private bool HasPath(ProcessStep from, ProcessStep target)
        {
            HashSet<ProcessStep> visited = [];
            Stack<ProcessStep> pending = new();
            pending.Push(from);
            while (pending.Count > 0)
            {
                ProcessStep current = pending.Pop();
                if (!visited.Add(current))
                {
                    continue;
                }

                if (current.Equals(target))
                {
                    return true;
                }

                foreach (ProcessStep successor in graph[current])
                {
                    pending.Push(successor);
                }
            }

            return false;
        }

        private Schedule CalculateSchedule(Process process)
        {
            Dictionary<ProcessStep, int> indegree = [];
            foreach (ProcessStep step in insertionOrder)
            {
                indegree.Add(step, 0);
            }

            foreach (IReadOnlyList<ProcessStep> successors in process.dependencyGraphValue.Values)
            {
                foreach (ProcessStep successor in successors)
                {
                    indegree[successor]++;
                }
            }

            Queue<ProcessStep> ready = new();
            foreach (ProcessStep step in insertionOrder)
            {
                if (indegree[step] == 0)
                {
                    ready.Enqueue(step);
                }
            }

            List<ProcessStep> ordered = [];
            while (ready.Count > 0)
            {
                ProcessStep current = ready.Dequeue();
                ordered.Add(current);
                foreach (ProcessStep successor in process.dependencyGraphValue[current])
                {
                    indegree[successor]--;
                    if (indegree[successor] == 0)
                    {
                        ready.Enqueue(successor);
                    }
                }
            }

            return new Schedule(ordered);
        }
    }

    internal sealed record class EdgeKey(ProcessStep fromValue, ProcessStep toValue)
    {
        internal ProcessStep From() => fromValue;

        internal ProcessStep To() => toValue;
    }
}
