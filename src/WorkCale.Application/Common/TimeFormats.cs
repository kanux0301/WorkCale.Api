namespace WorkCale.Application.Common;

public static class TimeFormats
{
    public const string HHmm = "HH:mm";
    public const string HHmmRegex = @"^\d{2}:\d{2}$";
    public const string HHmmMessage = "must be in HH:mm format.";

    public static TimeOnly ParseHHmm(string value) => TimeOnly.ParseExact(value, HHmm);
}
