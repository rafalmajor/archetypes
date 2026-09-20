using System;
using Xunit;
using static SoftwareArchetypes.Graphs.Scheduling.Fixtures;

namespace SoftwareArchetypes.Graphs.Scheduling;

public sealed class ScheduleTest
{
    [Fact]
    public void SimpleLinearProcess()
    {
        Schedule schedule = Process.CreateBuilder()
            .AddDependency(DRYING, MEASUREMENT, DependencyType.FinishToStart("Sample must be dry"))
            .AddDependency(MEASUREMENT, ANALYSIS, DependencyType.DataFlow("Spectrum"))
            .Build();

        Assert.Equal([DRYING, MEASUREMENT, ANALYSIS], schedule.Steps());
        Assert.Equal(DRYING, schedule.First());
        Assert.Equal(ANALYSIS, schedule.Last());
    }

    [Fact]
    public void ComplexProcessOrder()
    {
        Schedule schedule = Process.CreateBuilder()
            .AddDependency(DRYING, MEASUREMENT)
            .AddDependency(CALIBRATION, MEASUREMENT)
            .AddDependency(MEASUREMENT, ANALYSIS)
            .AddDependency(ANALYSIS, VALIDATION)
            .Build();

        Assert.Equal(5, schedule.Size());
        Assert.Equal(VALIDATION, schedule.Last());
        Assert.True(schedule.Steps().IndexOf(DRYING) < schedule.Steps().IndexOf(MEASUREMENT));
        Assert.True(schedule.Steps().IndexOf(CALIBRATION) < schedule.Steps().IndexOf(MEASUREMENT));
        Assert.True(schedule.Steps().IndexOf(MEASUREMENT) < schedule.Steps().IndexOf(ANALYSIS));
        Assert.True(schedule.Steps().IndexOf(ANALYSIS) < schedule.Steps().IndexOf(VALIDATION));
    }

    [Fact]
    public void SingleStepProcess()
    {
        Schedule schedule = Process.CreateBuilder().AddStep(DRYING).Build();

        Assert.Equal(1, schedule.Size());
        Assert.Equal(DRYING, schedule.First());
        Assert.Equal(DRYING, schedule.Last());
    }

    [Fact]
    public void DiamondPattern()
    {
        Schedule schedule = Process.CreateBuilder()
            .AddDependency(PREPARATION, PATH_1)
            .AddDependency(PREPARATION, PATH_2)
            .AddDependency(PATH_1, FINALIZATION)
            .AddDependency(PATH_2, FINALIZATION)
            .Build();

        Assert.Equal(4, schedule.Size());
        Assert.Equal(PREPARATION, schedule.First());
        Assert.Equal(FINALIZATION, schedule.Last());
        Assert.True(schedule.Steps().IndexOf(PREPARATION) < schedule.Steps().IndexOf(PATH_1));
        Assert.True(schedule.Steps().IndexOf(PREPARATION) < schedule.Steps().IndexOf(PATH_2));
        Assert.True(schedule.Steps().IndexOf(PATH_1) < schedule.Steps().IndexOf(FINALIZATION));
        Assert.True(schedule.Steps().IndexOf(PATH_2) < schedule.Steps().IndexOf(FINALIZATION));
    }

    [Fact]
    public void CyclicDependencyIsDetected()
    {
        Assert.Throws<ArgumentException>(() => Process.CreateBuilder()
            .AddDependency(STEP_1, STEP_2)
            .AddDependency(STEP_2, STEP_3)
            .AddDependency(STEP_3, STEP_1)
            .Build());
    }
}

internal static class ReadOnlyListExtensions
{
    internal static int IndexOf<T>(this System.Collections.Generic.IReadOnlyList<T> values, T value)
    {
        for (int index = 0; index < values.Count; index++)
        {
            if (Equals(values[index], value))
            {
                return index;
            }
        }

        return -1;
    }
}
