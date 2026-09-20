namespace com.softwarearchetypes.graphs.userjourney;

internal sealed record class UserJourneyId(string? valueValue)
{
    internal string? value() => valueValue;

    internal static UserJourneyId of(string? value) => new(value);
}
