using System;
using System.Collections.Generic;
using Xunit;

namespace com.softwarearchetypes.common;

public sealed class CommonUtilitiesTest
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("  ", false)]
    [InlineData("value", true)]
    public void isNotBlankMatchesJavaContract(string? value, bool expected) =>
        Assert.Equal(expected, StringUtils.isNotBlank(value));

    [Fact]
    public void keyValueMapFromReturnsEmptyMapForNull() =>
        Assert.Empty(CollectionTransformations.keyValueMapFrom(null));

    [Fact]
    public void keyValueMapFromMapsPairsAndKeepsLastDuplicate()
    {
        Dictionary<string, string?> result = CollectionTransformations.keyValueMapFrom(
            ["key", "first", "other", null, "key", "last"]);

        Assert.Equal(2, result.Count);
        Assert.Equal("last", result["key"]);
        Assert.Null(result["other"]);
    }

    [Fact]
    public void keyValueMapFromRejectsOddNumberOfArguments()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => CollectionTransformations.keyValueMapFrom(["key"]));

        Assert.Equal("The number of arguments must be even (key, productName, ...)", exception.Message);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("  ", 0)]
    [InlineData(null, 2)]
    public void keyValueMapFromRejectsBlankKeys(string? key, int expectedIndex)
    {
        string?[] parameters = expectedIndex == 0
            ? [key, "value"]
            : ["valid", "value", key, "value"];

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => CollectionTransformations.keyValueMapFrom(parameters));

        Assert.Equal($"Key (idx={expectedIndex}) cannot be empty or null", exception.Message);
    }

    [Fact]
    public void subtractReturnsIndependentSetDifference()
    {
        HashSet<int> minuend = [1, 2, 3];
        HashSet<int> subtrahend = [2, 4];

        ISet<int> result = CollectionTransformations.subtract(minuend, subtrahend);

        Assert.Equal(new HashSet<int> { 1, 3 }, result);
        Assert.Equal(new HashSet<int> { 1, 2, 3 }, minuend);
    }
}
