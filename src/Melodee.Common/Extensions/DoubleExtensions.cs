using System.Globalization;
using NodaTime;

namespace Melodee.Common.Extensions;

public static class DoubleExtensions
{
    public static string ToStringPadLeft(this double input, short padLeft, char padWith = '0')
    {
        return ToStringPadLeft(input as double?, padLeft, padWith) ?? string.Empty;
    }

    public static string? ToStringPadLeft(this double? input, short padLeft, char padWith = '0')
    {
        return input == null ? null : input!.ToString()!.PadLeft(padLeft, padWith);
    }

    public static int ToSeconds(this double seconds)
    {
        return Convert.ToInt32(seconds / 1000);
    }

    public static Duration ToDuration(this double? milliseconds)
    {
        return milliseconds == null ? Duration.Zero : Duration.FromMilliseconds(milliseconds.Value);
    }

    public static TimeSpan ToTimeSpan(this double? milliseconds)
    {
        return milliseconds == null ? TimeSpan.Zero : TimeSpan.FromMilliseconds(milliseconds.Value);
    }

    public static TimeSpan ToTimeSpan(this double milliseconds)
    {
        return milliseconds == 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(milliseconds);
    }

    public static Duration ToDuration(this double milliseconds)
    {
        return Duration.FromMilliseconds(milliseconds);
    }


    public static string ToFormattedDateTimeOffset(this double seconds, string? format = null)
    {
        var ts = TimeSpan.FromMilliseconds(seconds);
        return ts.ToString(format ?? "hh\\:mm\\:ss", CultureInfo.InvariantCulture);
    }
}
