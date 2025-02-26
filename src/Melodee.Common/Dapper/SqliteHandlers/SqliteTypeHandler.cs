using System.Data;
using Dapper;

namespace Melodee.Common.Dapper.SqliteHandlers;

public abstract class SqliteTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    // Parameters are converted by Microsoft.Data.Sqlite
    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        parameter.Value = value;
    }
}
