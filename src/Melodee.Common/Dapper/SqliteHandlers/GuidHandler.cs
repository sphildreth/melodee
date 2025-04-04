namespace Melodee.Common.Dapper.SqliteHandlers;

public class GuidHandler : SqliteTypeHandler<Guid>
{
    public override Guid Parse(object value)
    {
        return Guid.Parse((string)value);
    }
}
