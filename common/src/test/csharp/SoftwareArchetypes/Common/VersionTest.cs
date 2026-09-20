using Xunit;

namespace SoftwareArchetypes.Common;

public sealed class VersionTest
{
    [Fact]
    public void ShouldCreateInitialVersionWithZeroValue()
    {
        Version version = Version.Initial();

        Assert.NotNull(version);
        Assert.Equal(0L, version.Value());
    }

    [Fact]
    public void ShouldCreateVersionWithSpecificValue() => Assert.Equal(42L, Version.Of(42L).Value());

    [Fact]
    public void ShouldCreateVersionWithZeroValue() => Assert.Equal(0L, Version.Of(0L).Value());

    [Fact]
    public void ShouldCreateVersionWithNegativeValue() => Assert.Equal(-1L, Version.Of(-1L).Value());

    [Fact]
    public void ShouldCreateVersionWithMaxLongValue() => Assert.Equal(long.MaxValue, Version.Of(long.MaxValue).Value());

    [Fact]
    public void ShouldCreateVersionWithMinLongValue() => Assert.Equal(long.MinValue, Version.Of(long.MinValue).Value());

    [Fact]
    public void ShouldBeEqualWhenVersionsHaveSameValue()
    {
        Version firstVersion = Version.Of(10L);
        Version secondVersion = Version.Of(10L);

        Assert.Equal(firstVersion, secondVersion);
        Assert.Equal(firstVersion.GetHashCode(), secondVersion.GetHashCode());
    }

    [Fact]
    public void ShouldNotBeEqualWhenVersionsHaveDifferentValues() =>
        Assert.NotEqual(Version.Of(10L), Version.Of(20L));

    [Fact]
    public void ShouldHaveProperToStringRepresentation() =>
        Assert.Equal("Version[value=123]", Version.Of(123L).ToString());

    [Fact]
    public void ShouldInitialVersionBeEqualToVersionOfZero() =>
        Assert.Equal(Version.Initial(), Version.Of(0L));

    [Fact]
    public void ShouldCreateMultipleInitialVersionsWithSameValue()
    {
        Version firstInitial = Version.Initial();
        Version secondInitial = Version.Initial();

        Assert.Equal(firstInitial, secondInitial);
        Assert.Equal(0L, firstInitial.Value());
        Assert.Equal(0L, secondInitial.Value());
    }
}
