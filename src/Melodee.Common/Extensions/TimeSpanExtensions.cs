namespace Melodee.Common.Extensions;

public static class TimeSpanExtensions
{
    public static string ToYearDaysMinutesHours(this TimeSpan timeSpan)
    {
        // Calculate years (approximate)
        var years = timeSpan.Days / 365;
        var remainingDays = timeSpan.Days % 365;

        return
            $"{years.ToStringPadLeft(3)}:{remainingDays.ToStringPadLeft(3)}:{timeSpan.Hours.ToStringPadLeft(2)}:{timeSpan.Minutes.ToStringPadLeft(2)}";
    }
}
