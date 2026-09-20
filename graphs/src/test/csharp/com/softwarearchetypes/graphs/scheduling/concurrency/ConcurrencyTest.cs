using com.softwarearchetypes.graphs.scheduling;
using Xunit;

namespace com.softwarearchetypes.graphs.scheduling.concurrency;

public sealed class ConcurrencyTest
{
    private static readonly ProcessStep MEASUREMENT = new("Measurement");
    private static readonly ProcessStep CALIBRATION = new("Calibration");
    private static readonly ProcessStep VALIDATION = new("Validation");
    private static readonly ProcessStep FINAL_TEST = new("Final Test");

    [Fact]
    public void laboratoryStepsRequireMinimal3Environments()
    {
        ExecutionEnvironments environments = Concurrency.builder()
            .addConflict(MEASUREMENT, CALIBRATION)
            .addConflict(MEASUREMENT, VALIDATION)
            .addConflict(FINAL_TEST, MEASUREMENT)
            .addConflict(FINAL_TEST, CALIBRATION)
            .addConflict(FINAL_TEST, VALIDATION)
            .build();

        Assert.Equal(3, environments.environmentCount());
        Assert.True(environments.canRunConcurrently(CALIBRATION, VALIDATION));
        Assert.False(environments.canRunConcurrently(MEASUREMENT, CALIBRATION));
        Assert.False(environments.canRunConcurrently(MEASUREMENT, VALIDATION));
        Assert.False(environments.canRunConcurrently(FINAL_TEST, MEASUREMENT));
        Assert.False(environments.canRunConcurrently(FINAL_TEST, CALIBRATION));
        Assert.False(environments.canRunConcurrently(FINAL_TEST, VALIDATION));
    }

    [Fact]
    public void noConflictsRequiresOneEnvironment()
    {
        ProcessStep step1 = new("Step 1");
        ProcessStep step2 = new("Step 2");
        ProcessStep step3 = new("Step 3");

        ExecutionEnvironments environments = Concurrency.builder()
            .addStep(step1)
            .addStep(step2)
            .addStep(step3)
            .build();

        Assert.Equal(1, environments.environmentCount());
        Assert.True(environments.canRunConcurrently(step1, step2));
        Assert.True(environments.canRunConcurrently(step2, step3));
        Assert.True(environments.canRunConcurrently(step1, step3));
    }

    [Fact]
    public void completeConflictGraphRequiresMaxEnvironments()
    {
        ProcessStep step1 = new("Step 1");
        ProcessStep step2 = new("Step 2");
        ProcessStep step3 = new("Step 3");

        ExecutionEnvironments environments = Concurrency.builder()
            .addConflict(step1, step2)
            .addConflict(step1, step3)
            .addConflict(step2, step3)
            .build();

        Assert.Equal(3, environments.environmentCount());
        Assert.False(environments.canRunConcurrently(step1, step2));
        Assert.False(environments.canRunConcurrently(step1, step3));
        Assert.False(environments.canRunConcurrently(step2, step3));
    }

    [Fact]
    public void chainConflictsRequireTwoEnvironments()
    {
        ProcessStep stepA = new("Step A");
        ProcessStep stepB = new("Step B");
        ProcessStep stepC = new("Step C");

        ExecutionEnvironments environments = Concurrency.builder()
            .addConflict(stepA, stepB)
            .addConflict(stepB, stepC)
            .build();

        Assert.Equal(2, environments.environmentCount());
        Assert.False(environments.canRunConcurrently(stepA, stepB));
        Assert.False(environments.canRunConcurrently(stepB, stepC));
        Assert.True(environments.canRunConcurrently(stepA, stepC));
    }
}
