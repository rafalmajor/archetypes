using System;
using Xunit;

namespace SoftwareArchetypes.Common;

public sealed class PreconditionsTest
{
    [Fact]
    public void ShouldNotThrowExceptionWhenCheckArgumentWithTrueExpression() =>
        Preconditions.CheckArgument(true, "This should not be thrown");

    [Fact]
    public void ShouldThrowIllegalArgumentExceptionWhenCheckArgumentWithFalseExpression()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => Preconditions.CheckArgument(false, "Expression must be true"));

        Assert.Equal("Expression must be true", exception.Message);
    }

    [Fact]
    public void ShouldNotThrowExceptionWhenCheckNotNullWithNonNullValue() =>
        Preconditions.CheckNotNull("non-null value", "This should not be thrown");

    [Fact]
    public void ShouldThrowIllegalArgumentExceptionWhenCheckNotNullWithNullValue()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => Preconditions.CheckNotNull(null, "Value cannot be null"));

        Assert.Equal("Value cannot be null", exception.Message);
    }

    [Fact]
    public void ShouldThrowIllegalArgumentExceptionForComplexCondition()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => Preconditions.CheckArgument(15 >= 18, "Age must be at least 18"));

        Assert.Equal("Age must be at least 18", exception.Message);
    }

    [Fact]
    public void ShouldNotThrowExceptionForComplexCondition() =>
        Preconditions.CheckArgument(25 >= 18, "Age must be at least 18");
}
