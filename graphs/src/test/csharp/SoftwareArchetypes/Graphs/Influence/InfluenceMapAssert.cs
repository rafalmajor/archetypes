using Xunit;

namespace SoftwareArchetypes.Graphs.Influence;

internal sealed class InfluenceMapAssert
{
    private readonly InfluenceMap actual;

    private InfluenceMapAssert(InfluenceMap actual)
    {
        this.actual = actual;
    }

    internal static InfluenceMapAssert AssertThat(InfluenceMap actual) => new(actual);

    internal InfluenceMapAssert HasEdge(
        PhysicsProcess fromProcess,
        Laboratory fromLab,
        PhysicsProcess toProcess,
        Laboratory toLab)
    {
        Assert.True(
            actual.AsGraph().ContainsEdge(
                new InfluenceUnit(fromProcess, fromLab),
                new InfluenceUnit(toProcess, toLab)),
            $"Expected edge from ({fromProcess}, {fromLab}) to ({toProcess}, {toLab})");
        return this;
    }

    internal InfluenceMapAssert HasEdgeCount(int expectedCount)
    {
        Assert.Equal(expectedCount, actual.AsGraph().Edges().Count);
        return this;
    }
}
