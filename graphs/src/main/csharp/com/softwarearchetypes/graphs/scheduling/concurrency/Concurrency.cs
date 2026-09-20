using System.Collections.Generic;
using com.softwarearchetypes.graphs.scheduling;

namespace com.softwarearchetypes.graphs.scheduling.concurrency;

internal sealed class Concurrency
{
    private Concurrency()
    {
    }

    internal static Builder builder() => new();

    internal sealed class Builder
    {
        private readonly List<ProcessStep> insertionOrder = [];
        private readonly Dictionary<ProcessStep, HashSet<ProcessStep>> graph = [];

        internal Builder addStep(ProcessStep step)
        {
            if (!graph.ContainsKey(step))
            {
                graph.Add(step, []);
                insertionOrder.Add(step);
            }

            return this;
        }

        internal Builder addConflict(ProcessStep step1, ProcessStep step2)
        {
            addStep(step1);
            addStep(step2);
            graph[step1].Add(step2);
            graph[step2].Add(step1);
            return this;
        }

        internal ExecutionEnvironments build()
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