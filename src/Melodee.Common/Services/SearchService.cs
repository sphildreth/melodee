using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Models.Search;
using Melodee.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models.Collection;

namespace Melodee.Common.Services;

public sealed class SearchService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    ArtistService artistService,
    AlbumService albumService,
    SongService songService)
    : ServiceBase(logger, cacheManager, contextFactory)
{

    public async Task<OperationResult<SearchResult>> DoSearchAsync(string searchTerm, short maxResults, SearchInclude include, CancellationToken cancellationToken = default)
    {
        List<ArtistDataInfo> artists = new();
        List<AlbumDataInfo> albums = new();
        List<SongDataInfo> songs = new();

        if (searchTerm.Nullify() == null)
        {
            return new OperationResult<SearchResult>("No Search Term Provided")
            {
                Data = new SearchResult(artists.ToArray(), albums.ToArray(), songs.ToArray())
            };
        }
        
        var searchTermNormalized = searchTerm.ToNormalizedString() ?? searchTerm;
        
        if (include.HasFlag(SearchInclude.Artists))
        {
            var artistResult = await artistService.ListAsync(new PagedRequest
            {
                Page = 1,
                PageSize = maxResults,
                FilterBy =
                [
                    new FilterOperatorInfo(nameof(ArtistDataInfo.NameNormalized), FilterOperator.Contains, searchTermNormalized),
                    new FilterOperatorInfo(nameof(ArtistDataInfo.AlternateNames), FilterOperator.Contains, searchTermNormalized, FilterOperatorInfo.OrJoinOperator)
                ]
            }, cancellationToken);
            artists = artistResult.Data.ToList() ?? [];
        }
        if (include.HasFlag(SearchInclude.Albums))
        {
            var albumResult = await albumService.ListAsync(new PagedRequest
            {
                Page = 1,
                PageSize = maxResults,
                FilterBy =
                [
                    new FilterOperatorInfo(nameof(AlbumDataInfo.NameNormalized), FilterOperator.Contains, searchTermNormalized),
                    new FilterOperatorInfo(nameof(AlbumDataInfo.AlternateNames), FilterOperator.Contains, searchTermNormalized, FilterOperatorInfo.OrJoinOperator)
                ]
            }, cancellationToken);
            albums = albumResult.Data.ToList() ?? [];
        }      
        if (include.HasFlag(SearchInclude.Songs))
        {
            var songResult = await songService.ListAsync(new PagedRequest
            {
                Page = 1,
                PageSize = maxResults,
                FilterBy =
                [
                    new FilterOperatorInfo(nameof(SongDataInfo.TitleNormalized), FilterOperator.Contains, searchTermNormalized)                ]
            }, cancellationToken);
            songs = songResult.Data.ToList() ?? [];
        }         
        return new OperationResult<SearchResult>
        {
            Data = new SearchResult(artists.ToArray(), albums.ToArray(), songs.ToArray())
        };
    }
}
