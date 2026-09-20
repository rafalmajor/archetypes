using Xunit;

namespace SoftwareArchetypes.Common;

public sealed class PairTest
{
    [Fact]
    public void ShouldCreatePairWithBothValues()
    {
        Pair<string> pair = new("first", "second");

        Assert.NotNull(pair);
        Assert.Equal("first", pair.First());
        Assert.Equal("second", pair.Second());
    }

    [Fact]
    public void ShouldCreatePairWithNullValues()
    {
        Pair<string> pair = new(null, null);

        Assert.NotNull(pair);
        Assert.Null(pair.First());
        Assert.Null(pair.Second());
    }

    [Fact]
    public void ShouldCreatePairWithDifferentTypes()
    {
        Pair<int> pair = new(42, 100);

        Assert.Equal(42, pair.First());
        Assert.Equal(100, pair.Second());
    }

    [Fact]
    public void ShouldBeEqualWhenBothPairsHaveSameValues()
    {
        Pair<string> firstPair = new("A", "B");
        Pair<string> secondPair = new("A", "B");

        Assert.Equal(firstPair, secondPair);
        Assert.Equal(firstPair.GetHashCode(), secondPair.GetHashCode());
    }

    [Fact]
    public void ShouldNotBeEqualWhenPairsHaveDifferentFirstValue() =>
        Assert.NotEqual(new Pair<string>("A", "B"), new Pair<string>("C", "B"));

    [Fact]
    public void ShouldNotBeEqualWhenPairsHaveDifferentSecondValue() =>
        Assert.NotEqual(new Pair<string>("A", "B"), new Pair<string>("A", "C"));

    [Fact]
    public void ShouldNotBeEqualWhenPairsHaveDifferentValues() =>
        Assert.NotEqual(new Pair<string>("A", "B"), new Pair<string>("C", "D"));

    [Fact]
    public void ShouldHaveProperToStringRepresentation() =>
        Assert.Equal("Pair[first=first, second=second]", new Pair<string>("first", "second").ToString());

    [Fact]
    public void ShouldCreatePairWithSameValueForBothElements()
    {
        Pair<string> pair = new("same", "same");

        Assert.Equal("same", pair.First());
        Assert.Equal("same", pair.Second());
        Assert.Equal(pair.First(), pair.Second());
    }
}
