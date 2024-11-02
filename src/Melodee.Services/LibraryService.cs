using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Filtering;
using Melodee.Services.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Services;

public class LibraryService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{

    public async Task<MelodeeModels.OperationResult<Library>> GetInboundLibraryAsync(CancellationToken cancellationToken = default)
    {
        var library = await LibraryByType((int)LibraryType.Inbound, cancellationToken);
        if (library == null)
        {
            throw new Exception("Inbound library not found. A 'Library' record must be setup for a inbound library.");
        }
        return new MelodeeModels.OperationResult<Library>
        {
            Data = library
        }; 
    }
    
    public async Task<MelodeeModels.OperationResult<Library>> GetLibraryAsync(CancellationToken cancellationToken = default)
    {
        var library = await LibraryByType((int)LibraryType.Library, cancellationToken);
        if (library == null)
        {
            throw new Exception("Library not found. A 'Library' record must be setup for a library.");
        }
        return new MelodeeModels.OperationResult<Library>
        {
            Data = library
        }; 
    }

    private async Task<Library> LibraryByType(int type, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = $"SELECT * FROM \"Libraries\" WHERE \"Type\" = {type};";
            return await dbConn
                .QuerySingleAsync<Library>(sql)
                .ConfigureAwait(false);         
        }
    }
    
    public async Task<MelodeeModels.OperationResult<Library>> GetStagingLibraryAsync(CancellationToken cancellationToken = default)
    {
        var library = await LibraryByType((int)LibraryType.Staging, cancellationToken);
        if (library == null)
        {
            throw new Exception("Staging library not found. A 'Library' record must be setup for a staging library.");
        }
        return new MelodeeModels.OperationResult<Library>
        {
            Data = library
        };
    }

    public async Task<MelodeeModels.PagedResult<Library>> ListAsync(MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int libariesCount = 0;
        Library[] libraries = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                var orderBy = pagedRequest.OrderByValue();                
                var dbConn = scopedContext.Database.GetDbConnection();
                var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"Libraries\"");
                libariesCount = await dbConn
                    .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                    .ConfigureAwait(false);
                if (!pagedRequest.IsTotalCountOnlyRequest)
                {
                    var listSqlParts = pagedRequest.FilterByParts("SELECT * FROM \"Libraries\"");
                    var listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                    if (dbConn is SqliteConnection)
                    {
                        listSql = $"{listSqlParts.Item1 } ORDER BY {orderBy} LIMIT {pagedRequest.TakeValue} OFFSET {pagedRequest.SkipValue};";
                    }
                    libraries = (await dbConn
                        .QueryAsync<Library>(listSql, listSqlParts.Item2)
                        .ConfigureAwait(false)).ToArray();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to get libraries from database");
            }
        }

        return new MelodeeModels.PagedResult<Library>
        {
            TotalCount = libariesCount,
            TotalPages = pagedRequest.TotalPages(libariesCount),
            Data = libraries
        };
    }
}
