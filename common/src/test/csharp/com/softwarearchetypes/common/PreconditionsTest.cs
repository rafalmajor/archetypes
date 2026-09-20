using System;
using Xunit;

namespace com.softwarearchetypes.common;

public sealed class PreconditionsTest
{
    [Fact]
    public void shouldNotThrowExceptionWhenCheckArgumentWithTrueExpression() =>
        Preconditions.checkArgument(true, "This should not be thrown");

    [Fact]
    public void shouldThrowIllegalArgumentExceptionWhenCheckArgumentWithFalseExpression()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => Preconditions.checkArgument(false, "Expression must be true"));

        Assert.Equal("Expression must be true", exception.Message);
    }

    [Fact]
    public void shouldNotThrowExceptionWhenCheckNotNullWithNonNullValue() =>
        Preconditions.checkNotNull("non-null value", "This should not be thrown");

    [Fact]
    public void shouldThrowIllegalArgumentExceptionWhenCheckNotNullWithNullValue()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => Preconditions.checkNotNull(null, "Value cannot be null"));

        Assert.Equal("Value cannot be null", exception.Message);
    }

    [Fact]
    public void shouldThrowIllegalArgumentExceptionForComplexCondition()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => Preconditions.checkArgument(15 >= 18, "Age must be at least 18"));

        Assert.Equal("Age must be at least 18", exception.Message);
    }

    [Fact]
    public void shouldNotThrowExceptionForComplexCondition() =>
        Preconditions.checkArgument(25 >= 18, "Age must be at least 18");
}
