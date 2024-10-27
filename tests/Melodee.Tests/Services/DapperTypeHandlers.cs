using System.Data;
using Dapper;
using NodaTime;

/*
 *  These are only here for unit testing with SQlite. Melodee doesn't plan on supporting SQlite as a database.
 */


abstract class SqliteTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    // Parameters are converted by Microsoft.Data.Sqlite
    public override void SetValue(IDbDataParameter parameter, T? value)
        => parameter.Value = value;
}

class DateTimeOffsetHandler : SqliteTypeHandler<DateTimeOffset>
{
    public override DateTimeOffset Parse(object value)
        => DateTimeOffset.Parse((string)value);
}

class InstantHandler : SqliteTypeHandler<Instant>
{
    public override Instant Parse(object value)
    {
        if (value is DateTime dateTime)
        {
            var dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return Instant.FromDateTimeUtc(dt);
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            return Instant.FromDateTimeOffset(dateTimeOffset);
        }

        if (DateTime.TryParse(value as string, out var parsedDateTime))
        { 
            var dt = DateTime.SpecifyKind(parsedDateTime, DateTimeKind.Utc);
            return Instant.FromDateTimeUtc(dt);
        }
        
        throw new DataException("Cannot convert " + value.GetType() + " to NodaTime.Instant");
    }
}

class GuidHandler : SqliteTypeHandler<Guid>
{
    public override Guid Parse(object value)
        => Guid.Parse((string)value);
}

class TimeSpanHandler : SqliteTypeHandler<TimeSpan>
{
    public override TimeSpan Parse(object value)
        => TimeSpan.Parse((string)value);
}
