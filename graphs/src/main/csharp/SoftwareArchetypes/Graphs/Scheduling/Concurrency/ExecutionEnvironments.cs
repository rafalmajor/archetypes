using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SoftwareArchetypes.Graphs.Scheduling;

namespace SoftwareArchetypes.Graphs.Scheduling.Concurrency;

internal sealed class ExecutionEnvironments : IEquatable<ExecutionEnvironments>
{
    private readonly IReadOnlyDictionary<ProcessStep, int> stepToEnvironmentValue;

    internal ExecutionEnvironments(IDictionary<ProcessStep, int> stepToEnvironment)
    {
        stepToEnvironmentValue =
            new ReadOnlyDictionary<ProcessStep, int>(new Dictionary<ProcessStep, int>(stepToEnvironment));
    }

    internal IReadOnlyDictionary<ProcessStep, int> StepToEnvironment() => stepToEnvironmentValue;

    internal int EnvironmentCount()
    {
        int maximum = -1;
        foreach (int environment in stepToEnvironmentValue.Values)
        {
            maximum = Math.Max(maximum, environment);
        }

        return maximum + 1;
    }

    internal int? GetEnvironment(ProcessStep step) =>
        stepToEnvironmentValue.TryGetValue(step, out int environment) ? environment : null;

    internal IReadOnlySet<ProcessStep> GetStepsInEnvironment(int environment)
    {
        HashSet<ProcessStep> steps = [];
        foreach ((ProcessStep step, int assignedEnvironment) in stepToEnvironmentValue)
        {
            if (assignedEnvironment == environment)
            {
                steps.Add(step);
            }
        }

        return steps;
    }

    internal bool CanRunConcurrently(ProcessStep step1, ProcessStep step2)
    {
        int? environment1 = GetEnvironment(step1);
        int? environment2 = GetEnvironment(step2);
        return environment1 is not null && environment1 == environment2;
    }

    public bool Equals(ExecutionEnvironments? other)
    {
        if (other is null || stepToEnvironmentValue.Count != other.stepToEnvironmentValue.Count)
        {
            return false;
        }

        foreach ((ProcessStep step, int environment) in stepToEnvironmentValue)
        {
            if (!other.stepToEnvironmentValue.TryGetValue(step, out int otherEnvironment) ||
                environment != otherEnvironment)
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is ExecutionEnvironments other && Equals(other);

    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach ((ProcessStep step, int environment) in stepToEnvironmentValue)
        {
            hash.Add(step);
            hash.Add(environment);
        }

        return hash.ToHashCode();
    }
}
