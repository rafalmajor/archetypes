using System;
using System.Collections.Generic;
using Xunit;

namespace SoftwareArchetypes.Common;

public sealed class CommonUtilitiesTest
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("  ", false)]
    [InlineData("value", true)]
    public void IsNotBlankMatchesJavaContract(string? value, bool expected) =>
        Assert.Equal(expected, StringUtils.IsNotBlank(value));

    [Fact]
    public void KeyValueMapFromReturnsEmptyMapForNull() =>
        Assert.Empty(CollectionTransformations.KeyValueMapFrom(null));

    [Fact]
    public void KeyValueMapFromMapsPairsAndKeepsLastDuplicate()
    {
        Dictionary<string, string?> result = CollectionTransformations.KeyValueMapFrom(
            ["key", "first", "other", null, "key", "last"]);

        Assert.Equal(2, result.Count);
        Assert.Equal("last", result["key"]);
        Assert.Null(result["other"]);
    }

    [Fact]
    public void KeyValueMapFromRejectsOddNumberOfArguments()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => CollectionTransformations.KeyValueMapFrom(["key"]));

        Assert.Equal("The number of arguments must be even (key, productName, ...)", exception.Message);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("  ", 0)]
    [InlineData(null, 2)]
    public void KeyValueMapFromRejectsBlankKeys(string? key, int expectedIndex)
    {
        string?[] parameters = expectedIndex == 0
            ? [key, "value"]
            : ["valid", "value", key, "value"];

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => CollectionTransformations.KeyValueMapFrom(parameters));

        Assert.Equal($"Key (idx={expectedIndex}) cannot be empty or null", exception.Message);
    }

    [Fact]
    public void SubtractReturnsIndependentSetDifference()
    {
        HashSet<int> minuend = [1, 2, 3];
        HashSet<int> subtrahend = [2, 4];

        ISet<int> result = CollectionTransformations.Subtract(minuend, subtrahend);

        Assert.Equal(new HashSet<int> { 1, 3 }, result);
        Assert.Equal(new HashSet<int> { 1, 2, 3 }, minuend);
    }
}
