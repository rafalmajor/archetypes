using System;
using Xunit;
using static com.softwarearchetypes.graphs.scheduling.Fixtures;

namespace com.softwarearchetypes.graphs.scheduling;

public sealed class ScheduleTest
{
    [Fact]
    public void simpleLinearProcess()
    {
        Schedule schedule = Process.builder()
            .addDependency(DRYING, MEASUREMENT, DependencyType.finishToStart("Sample must be dry"))
            .addDependency(MEASUREMENT, ANALYSIS, DependencyType.dataFlow("Spectrum"))
            .build();

        Assert.Equal([DRYING, MEASUREMENT, ANALYSIS], schedule.steps());
        Assert.Equal(DRYING, schedule.first());
        Assert.Equal(ANALYSIS, schedule.last());
    }

    [Fact]
    public void complexProcessOrder()
    {
        Schedule schedule = Process.builder()
            .addDependency(DRYING, MEASUREMENT)
            .addDependency(CALIBRATION, MEASUREMENT)
            .addDependency(MEASUREMENT, ANALYSIS)
            .addDependency(ANALYSIS, VALIDATION)
            .build();

        Assert.Equal(5, schedule.size());
        Assert.Equal(VALIDATION, schedule.last());
        Assert.True(schedule.steps().IndexOf(DRYING) < schedule.steps().IndexOf(MEASUREMENT));
        Assert.True(schedule.steps().IndexOf(CALIBRATION) < schedule.steps().IndexOf(MEASUREMENT));
        Assert.True(schedule.steps().IndexOf(MEASUREMENT) < schedule.steps().IndexOf(ANALYSIS));
        Assert.True(schedule.steps().IndexOf(ANALYSIS) < schedule.steps().IndexOf(VALIDATION));
    }

    [Fact]
    public void singleStepProcess()
    {
        Schedule schedule = Process.builder().addStep(DRYING).build();

        Assert.Equal(1, schedule.size());
        Assert.Equal(DRYING, schedule.first());
        Assert.Equal(DRYING, schedule.last());
    }

    [Fact]
    public void diamondPattern()
    {
        Schedule schedule = Process.builder()
            .addDependency(PREPARATION, PATH_1)
            .addDependency(PREPARATION, PATH_2)
            .addDependency(PATH_1, FINALIZATION)
            .addDependency(PATH_2, FINALIZATION)
            .build();

        Assert.Equal(4, schedule.size());
        Assert.Equal(PREPARATION, schedule.first());
        Assert.Equal(FINALIZATION, schedule.last());
        Assert.True(schedule.steps().IndexOf(PREPARATION) < schedule.steps().IndexOf(PATH_1));
        Assert.True(schedule.steps().IndexOf(PREPARATION) < schedule.steps().IndexOf(PATH_2));
        Assert.True(schedule.steps().IndexOf(PATH_1) < schedule.steps().IndexOf(FINALIZATION));
        Assert.True(schedule.steps().IndexOf(PATH_2) < schedule.steps().IndexOf(FINALIZATION));
    }

    [Fact]
    public void cyclicDependencyIsDetected()
    {
        Assert.Throws<ArgumentException>(() => Process.builder()
            .addDependency(STEP_1, STEP_2)
            .addDependency(STEP_2, STEP_3)
            .addDependency(STEP_3, STEP_1)
            .build());
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