namespace Melodee.Common.Extensions;

public static class DateTimeOffsetExtensions
{
    public static string ToXmlSchemaDateTimeFormat(this DateTimeOffset dateTimeOffset) => dateTimeOffset.ToString("yyyy-MM-ddThh:mm:ss");
}