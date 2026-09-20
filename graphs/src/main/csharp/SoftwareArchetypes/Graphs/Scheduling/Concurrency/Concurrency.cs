using System.Collections.Generic;
using SoftwareArchetypes.Graphs.Scheduling;

namespace SoftwareArchetypes.Graphs.Scheduling.Concurrency;

internal sealed class Concurrency
{
    private Concurrency()
    {
    }

    internal static Builder CreateBuilder() => new();

    internal sealed class Builder
    {
        private readonly List<ProcessStep> insertionOrder = [];
        private readonly Dictionary<ProcessStep, HashSet<ProcessStep>> graph = [];

        internal Builder AddStep(ProcessStep step)
        {
            if (!graph.ContainsKey(step))
            {
                graph.Add(step, []);
                insertionOrder.Add(step);
            }

            return this;
        }

        internal Builder AddConflict(ProcessStep step1, ProcessStep step2)
        {
            AddStep(step1);
            AddStep(step2);
            graph[step1].Add(step2);
            graph[step2].Add(step1);
            return this;
        }

        internal ExecutionEnvironments Build()
        {
            Dictionary<ProcessStep, int> stepToEnvironment = [];
            foreach (ProcessStep step in insertionOrder)
            {
                HashSet<int> unavailable = [];
                foreach (ProcessStep conflict in graph[step])
                {
                    if (stepToEnvironment.TryGetValue(conflict, out int environment))
                    {
                        unavailable.Add(environment);
                    }
                }

                int selected = 0;
                while (unavailable.Contains(selected))
                {
                    selected++;
                }

                stepToEnvironment.Add(step, selected);
            }

            return new ExecutionEnvironments(stepToEnvironment);
        }
    }
}
