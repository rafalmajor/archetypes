namespace com.softwarearchetypes.common;

public static class StringUtils
{
    public static bool isNotBlank(string? value) => !string.IsNullOrWhiteSpace(value);
}
