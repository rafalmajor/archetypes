using Xunit;

namespace com.softwarearchetypes.graphs.influence;

internal sealed class InfluenceMapAssert
{
    private readonly InfluenceMap actual;

    private InfluenceMapAssert(InfluenceMap actual)
    {
        this.actual = actual;
    }

    internal static InfluenceMapAssert assertThat(InfluenceMap actual) => new(actual);

    internal InfluenceMapAssert hasEdge(
        PhysicsProcess fromProcess,
        Laboratory fromLab,
        PhysicsProcess toProcess,
        Laboratory toLab)
    {
        Assert.True(
            actual.asGraph().containsEdge(
                new InfluenceUnit(fromProcess, fromLab),
                new InfluenceUnit(toProcess, toLab)),
            $"Expected edge from ({fromProcess}, {fromLab}) to ({toProcess}, {toLab})");
        return this;
    }

    internal InfluenceMapAssert hasEdgeCount(int expectedCount)
    {
        Assert.Equal(expectedCount, actual.asGraph().edges().Count);
        return this;
    }
}