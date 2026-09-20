namespace SoftwareArchetypes.Graphs.UserJourney;

internal sealed record class UserJourneyId(string? valueValue)
{
    internal string? Value() => valueValue;

    internal static UserJourneyId Of(string? value) => new(value);
}
