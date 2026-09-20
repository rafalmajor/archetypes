using Xunit;

namespace com.softwarearchetypes.graphs.cycles.math;

public sealed class GraphIntersectionTest
{
    [Fact]
    public void intersectionContainsOnlyCommonEdges()
    {
        Graph<string, string> first = cycleGraph();
        Graph<string, string> second = new Graph<string, string>()
            .addEdge(edge("A", "B", "edge1"))
            .addEdge(edge("B", "C", "edge2"));

        Graph<string, string> intersection = first.intersection(second);

        Assert.True(intersection.hasEdge(node("A"), node("B")));
        Assert.True(intersection.hasEdge(node("B"), node("C")));
        Assert.False(intersection.hasEdge(node("C"), node("A")));
    }

    [Fact]
    public void intersectionFindsCycleOnlyWhenAllEdgesInBoth()
    {
        Path<string, string>? cycle = cycleGraph().intersection(cycleGraph()).findFirstCycle();

        Assert.NotNull(cycle);
        Assert.Equal(3, cycle.edges().Count);
    }

    [Fact]
    public void intersectionDoesNotFindCycleWhenEdgeMissingInSecondGraph()
    {
        Graph<string, string> incomplete = new Graph<string, string>()
            .addEdge(edge("A", "B", "edge1"))
            .addEdge(edge("B", "C", "edge2"));

        Assert.Null(cycleGraph().intersection(incomplete).findFirstCycle());
    }

    [Fact]
    public void intersectionOfEmptyGraphsIsEmpty() =>
        Assert.Null(new Graph<string, string>().intersection(new Graph<string, string>()).findFirstCycle());

    [Fact]
    public void intersectionWithEmptyGraphIsEmpty()
    {
        Graph<string, string> intersection = new Graph<string, string>()
            .addEdge(edge("A", "B", "edge1"))
            .addEdge(edge("B", "C", "edge2"))
            .intersection(new Graph<string, string>());

        Assert.False(intersection.hasEdge(node("A"), node("B")));
        Assert.False(intersection.hasEdge(node("B"), node("C")));
    }

    private static Graph<string, string> cycleGraph() => new Graph<string, string>()
        .addEdge(edge("A", "B", "edge1"))
        .addEdge(edge("B", "C", "edge2"))
        .addEdge(edge("C", "A", "edge3"));

    private static Node<string> node(string value) => new(value);

    private static Edge<string, string> edge(string from, string to, string property) =>
        new(node(from), node(to), property);
}
