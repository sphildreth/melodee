using System.Diagnostics;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.Search;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services;

public sealed class SearchService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    ArtistService artistService,
    AlbumService albumService,
    SongService songService,
    IMusicBrainzRepository musicBrainzRepository)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public async Task<OperationResult<SearchResult>> DoSearchAsync(Guid userApiKey, string? userAgent, string? searchTerm, short maxResults, SearchInclude include, CancellationToken cancellationToken = default)
    {
        List<ArtistDataInfo> artists = new();
        List<AlbumDataInfo> albums = new();
        List<SongDataInfo> songs = new();
        List<ArtistDataInfo> musicBrainzArtists = new();

        if (searchTerm.Nullify() == null)
        {
            return new OperationResult<SearchResult>("No Search Term Provided")
            {
                Data = new SearchResult([], [], [], [])
            };
        }

        var startTicks = Stopwatch.GetTimestamp();
        
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
                    new FilterOperatorInfo(nameof(SongDataInfo.TitleNormalized), FilterOperator.Contains, searchTermNormalized)
                ]
            }, cancellationToken);
            songs = songResult.Data.ToList() ?? [];
        }

        if (include.HasFlag(SearchInclude.MusicBrainz))
        {
            var searchResult = await musicBrainzRepository.SearchArtist(new ArtistQuery
            {
                Name = searchTerm
            }, maxResults, cancellationToken);
            musicBrainzArtists = searchResult.Data
                .Where(x => x.MusicBrainzId != null)
                .Select(x => ArtistDataInfo.BlankArtistDataInfo with
                {
                    ApiKey = x.MusicBrainzId!.Value,
                    Name = x.Name,
                    NameNormalized = x.Name.ToNormalizedString() ?? x.Name
                })
                .ToList();
        }

        var elapsedTime = Stopwatch.GetElapsedTime(startTicks);
        
        // TODO EventBus
        // await searchEventPublisher.Publish(new Event<SearchHistoryEvent>(new SearchHistoryEvent
        // {
        //     CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
        //     ByUserApiKey = userApiKey,
        //     ByUserAgent = userAgent,
        //     SearchQuery = searchTerm?.ToBase64(),
        //     FoundArtistsCount = artists.Count,
        //     FoundAlbumsCount = albums.Count,
        //     FoundSongsCount = songs.Count,
        //     FoundOtherItems = musicBrainzArtists.Count,
        //     SearchDurationInMs = elapsedTime.TotalMilliseconds
        // }), cancellationToken).ConfigureAwait(false);
        return new OperationResult<SearchResult>
        {
            Data = new SearchResult(artists.ToArray(), albums.ToArray(), songs.ToArray(), musicBrainzArtists.ToArray())
        };
    }
}
