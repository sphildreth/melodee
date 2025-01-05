namespace Melodee.Common.Extensions;

public static class DateTimeExtensions
{
    public static string ToEtag(this DateTime dateTime)
    => dateTime.Ticks.ToString();
}
