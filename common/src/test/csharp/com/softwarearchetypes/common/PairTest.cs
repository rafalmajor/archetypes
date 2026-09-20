using Xunit;

namespace com.softwarearchetypes.common;

public sealed class PairTest
{
    [Fact]
    public void shouldCreatePairWithBothValues()
    {
        Pair<string> pair = new("first", "second");

        Assert.NotNull(pair);
        Assert.Equal("first", pair.first());
        Assert.Equal("second", pair.second());
    }

    [Fact]
    public void shouldCreatePairWithNullValues()
    {
        Pair<string> pair = new(null, null);

        Assert.NotNull(pair);
        Assert.Null(pair.first());
        Assert.Null(pair.second());
    }

    [Fact]
    public void shouldCreatePairWithDifferentTypes()
    {
        Pair<int> pair = new(42, 100);

        Assert.Equal(42, pair.first());
        Assert.Equal(100, pair.second());
    }

    [Fact]
    public void shouldBeEqualWhenBothPairsHaveSameValues()
    {
        Pair<string> firstPair = new("A", "B");
        Pair<string> secondPair = new("A", "B");

        Assert.Equal(firstPair, secondPair);
        Assert.Equal(firstPair.GetHashCode(), secondPair.GetHashCode());
    }

    [Fact]
    public void shouldNotBeEqualWhenPairsHaveDifferentFirstValue() =>
        Assert.NotEqual(new Pair<string>("A", "B"), new Pair<string>("C", "B"));

    [Fact]
    public void shouldNotBeEqualWhenPairsHaveDifferentSecondValue() =>
        Assert.NotEqual(new Pair<string>("A", "B"), new Pair<string>("A", "C"));

    [Fact]
    public void shouldNotBeEqualWhenPairsHaveDifferentValues() =>
        Assert.NotEqual(new Pair<string>("A", "B"), new Pair<string>("C", "D"));

    [Fact]
    public void shouldHaveProperToStringRepresentation() =>
        Assert.Equal("Pair[first=first, second=second]", new Pair<string>("first", "second").ToString());

    [Fact]
    public void shouldCreatePairWithSameValueForBothElements()
    {
        Pair<string> pair = new("same", "same");

        Assert.Equal("same", pair.first());
        Assert.Equal("same", pair.second());
        Assert.Equal(pair.first(), pair.second());
    }
}
