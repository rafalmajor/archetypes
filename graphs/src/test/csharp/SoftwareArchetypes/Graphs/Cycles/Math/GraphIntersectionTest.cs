using Xunit;

namespace SoftwareArchetypes.Graphs.Cycles.Math;

public sealed class GraphIntersectionTest
{
    [Fact]
    public void IntersectionContainsOnlyCommonEdges()
    {
        Graph<string, string> first = CycleGraph();
        Graph<string, string> second = new Graph<string, string>()
            .AddEdge(Edge("A", "B", "edge1"))
            .AddEdge(Edge("B", "C", "edge2"));

        Graph<string, string> intersection = first.Intersection(second);

        Assert.True(intersection.HasEdge(Node("A"), Node("B")));
        Assert.True(intersection.HasEdge(Node("B"), Node("C")));
        Assert.False(intersection.HasEdge(Node("C"), Node("A")));
    }

    [Fact]
    public void IntersectionFindsCycleOnlyWhenAllEdgesInBoth()
    {
        Path<string, string>? cycle = CycleGraph().Intersection(CycleGraph()).FindFirstCycle();

        Assert.NotNull(cycle);
        Assert.Equal(3, cycle.Edges().Count);
    }

    [Fact]
    public void IntersectionDoesNotFindCycleWhenEdgeMissingInSecondGraph()
    {
        Graph<string, string> incomplete = new Graph<string, string>()
            .AddEdge(Edge("A", "B", "edge1"))
            .AddEdge(Edge("B", "C", "edge2"));

        Assert.Null(CycleGraph().Intersection(incomplete).FindFirstCycle());
    }

    [Fact]
    public void IntersectionOfEmptyGraphsIsEmpty() =>
        Assert.Null(new Graph<string, string>().Intersection(new Graph<string, string>()).FindFirstCycle());

    [Fact]
    public void IntersectionWithEmptyGraphIsEmpty()
    {
        Graph<string, string> intersection = new Graph<string, string>()
            .AddEdge(Edge("A", "B", "edge1"))
            .AddEdge(Edge("B", "C", "edge2"))
            .Intersection(new Graph<string, string>());

        Assert.False(intersection.HasEdge(Node("A"), Node("B")));
        Assert.False(intersection.HasEdge(Node("B"), Node("C")));
    }

    private static Graph<string, string> CycleGraph() => new Graph<string, string>()
        .AddEdge(Edge("A", "B", "edge1"))
        .AddEdge(Edge("B", "C", "edge2"))
        .AddEdge(Edge("C", "A", "edge3"));

    private static Node<string> Node(string value) => new(value);

    private static Edge<string, string> Edge(string from, string to, string property) =>
        new(Node(from), Node(to), property);
}
