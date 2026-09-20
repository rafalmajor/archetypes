using SoftwareArchetypes.Graphs.Scheduling;
using Xunit;

namespace SoftwareArchetypes.Graphs.Scheduling.Concurrency;

public sealed class ConcurrencyTest
{
    private static readonly ProcessStep MEASUREMENT = new("Measurement");
    private static readonly ProcessStep CALIBRATION = new("Calibration");
    private static readonly ProcessStep VALIDATION = new("Validation");
    private static readonly ProcessStep FINAL_TEST = new("Final Test");

    [Fact]
    public void LaboratoryStepsRequireMinimal3Environments()
    {
        ExecutionEnvironments environments = Concurrency.CreateBuilder()
            .AddConflict(MEASUREMENT, CALIBRATION)
            .AddConflict(MEASUREMENT, VALIDATION)
            .AddConflict(FINAL_TEST, MEASUREMENT)
            .AddConflict(FINAL_TEST, CALIBRATION)
            .AddConflict(FINAL_TEST, VALIDATION)
            .Build();

        Assert.Equal(3, environments.EnvironmentCount());
        Assert.True(environments.CanRunConcurrently(CALIBRATION, VALIDATION));
        Assert.False(environments.CanRunConcurrently(MEASUREMENT, CALIBRATION));
        Assert.False(environments.CanRunConcurrently(MEASUREMENT, VALIDATION));
        Assert.False(environments.CanRunConcurrently(FINAL_TEST, MEASUREMENT));
        Assert.False(environments.CanRunConcurrently(FINAL_TEST, CALIBRATION));
        Assert.False(environments.CanRunConcurrently(FINAL_TEST, VALIDATION));
    }

    [Fact]
    public void NoConflictsRequiresOneEnvironment()
    {
        ProcessStep step1 = new("Step 1");
        ProcessStep step2 = new("Step 2");
        ProcessStep step3 = new("Step 3");

        ExecutionEnvironments environments = Concurrency.CreateBuilder()
            .AddStep(step1)
            .AddStep(step2)
            .AddStep(step3)
            .Build();

        Assert.Equal(1, environments.EnvironmentCount());
        Assert.True(environments.CanRunConcurrently(step1, step2));
        Assert.True(environments.CanRunConcurrently(step2, step3));
        Assert.True(environments.CanRunConcurrently(step1, step3));
    }

    [Fact]
    public void CompleteConflictGraphRequiresMaxEnvironments()
    {
        ProcessStep step1 = new("Step 1");
        ProcessStep step2 = new("Step 2");
        ProcessStep step3 = new("Step 3");

        ExecutionEnvironments environments = Concurrency.CreateBuilder()
            .AddConflict(step1, step2)
            .AddConflict(step1, step3)
            .AddConflict(step2, step3)
            .Build();

        Assert.Equal(3, environments.EnvironmentCount());
        Assert.False(environments.CanRunConcurrently(step1, step2));
        Assert.False(environments.CanRunConcurrently(step1, step3));
        Assert.False(environments.CanRunConcurrently(step2, step3));
    }

    [Fact]
    public void ChainConflictsRequireTwoEnvironments()
    {
        ProcessStep stepA = new("Step A");
        ProcessStep stepB = new("Step B");
        ProcessStep stepC = new("Step C");

        ExecutionEnvironments environments = Concurrency.CreateBuilder()
            .AddConflict(stepA, stepB)
            .AddConflict(stepB, stepC)
            .Build();

        Assert.Equal(2, environments.EnvironmentCount());
        Assert.False(environments.CanRunConcurrently(stepA, stepB));
        Assert.False(environments.CanRunConcurrently(stepB, stepC));
        Assert.True(environments.CanRunConcurrently(stepA, stepC));
    }
}
