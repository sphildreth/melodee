namespace Melodee.Common.Dapper.SqliteHandlers;

public class TimeSpanHandler : SqliteTypeHandler<TimeSpan>
{
    public override TimeSpan Parse(object value)
    {
        return TimeSpan.Parse((string)value);
    }
}
