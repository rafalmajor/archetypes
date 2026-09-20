using Xunit;

namespace com.softwarearchetypes.common;

public sealed class VersionTest
{
    [Fact]
    public void shouldCreateInitialVersionWithZeroValue()
    {
        Version version = Version.initial();

        Assert.NotNull(version);
        Assert.Equal(0L, version.value());
    }

    [Fact]
    public void shouldCreateVersionWithSpecificValue() => Assert.Equal(42L, Version.of(42L).value());

    [Fact]
    public void shouldCreateVersionWithZeroValue() => Assert.Equal(0L, Version.of(0L).value());

    [Fact]
    public void shouldCreateVersionWithNegativeValue() => Assert.Equal(-1L, Version.of(-1L).value());

    [Fact]
    public void shouldCreateVersionWithMaxLongValue() => Assert.Equal(long.MaxValue, Version.of(long.MaxValue).value());

    [Fact]
    public void shouldCreateVersionWithMinLongValue() => Assert.Equal(long.MinValue, Version.of(long.MinValue).value());

    [Fact]
    public void shouldBeEqualWhenVersionsHaveSameValue()
    {
        Version firstVersion = Version.of(10L);
        Version secondVersion = Version.of(10L);

        Assert.Equal(firstVersion, secondVersion);
        Assert.Equal(firstVersion.GetHashCode(), secondVersion.GetHashCode());
    }

    [Fact]
    public void shouldNotBeEqualWhenVersionsHaveDifferentValues() =>
        Assert.NotEqual(Version.of(10L), Version.of(20L));

    [Fact]
    public void shouldHaveProperToStringRepresentation() =>
        Assert.Equal("Version[value=123]", Version.of(123L).ToString());

    [Fact]
    public void shouldInitialVersionBeEqualToVersionOfZero() =>
        Assert.Equal(Version.initial(), Version.of(0L));

    [Fact]
    public void shouldCreateMultipleInitialVersionsWithSameValue()
    {
        Version firstInitial = Version.initial();
        Version secondInitial = Version.initial();

        Assert.Equal(firstInitial, secondInitial);
        Assert.Equal(0L, firstInitial.value());
        Assert.Equal(0L, secondInitial.value());
    }
}
