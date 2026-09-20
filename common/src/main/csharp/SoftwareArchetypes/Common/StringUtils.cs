namespace SoftwareArchetypes.Common;

public static class StringUtils
{
    public static bool IsNotBlank(string? value) => !string.IsNullOrWhiteSpace(value);
}
