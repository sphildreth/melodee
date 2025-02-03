using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Serilog;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Http;

namespace Melodee.Common.Plugins.SearchEngine.Spotify;

public class Spotify(
    ILogger logger,
    IMelodeeConfiguration configuration,
    SettingService settingService)
    : IArtistSearchEnginePlugin, IArtistTopSongsSearchEnginePlugin, IAlbumImageSearchEnginePlugin, IArtistImageSearchEnginePlugin
{
    public async Task<OperationResult<ImageSearchResult[]?>> DoAlbumImageSearch(AlbumQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        var results = new List<ImageSearchResult>();

        try
        {
            var apiClientId = configuration.GetValue<string>(SettingRegistry.SearchEngineSpotifyApiKey);
            var apiClientSecret = configuration.GetValue<string>(SettingRegistry.SearchEngineSpotifyClientSecret);

            if (string.IsNullOrWhiteSpace(apiClientId) || string.IsNullOrWhiteSpace(apiClientSecret))
            {
                return new OperationResult<ImageSearchResult[]?>("Spotify API key not configured.")
                {
                    Data = []
                };
            }

            var apiAccessToken = configuration.GetValue<string>(SettingRegistry.SearchEngineSpotifyAccessToken);

            var config = SpotifyClientConfig.CreateDefault();

            if (string.IsNullOrWhiteSpace(apiAccessToken))
            {
                var request = new ClientCredentialsRequest(apiClientId, apiClientSecret);
                var response = await new OAuthClient(config).RequestToken(request, cancellationToken);
                apiAccessToken = response.AccessToken;
                await settingService.SetAsync(SettingRegistry.SearchEngineSpotifyAccessToken, apiAccessToken, cancellationToken);
            }

            var spotify = new SpotifyClient(config.WithToken(apiAccessToken));
            SearchResponse? searchResult = null;
            try
            {
                searchResult = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Album, query.Name), cancellationToken);
            }
            catch (APIUnauthorizedException)
            {
                var request = new ClientCredentialsRequest(apiClientId, apiClientSecret);
                var response = await new OAuthClient(config).RequestToken(request, cancellationToken);
                apiAccessToken = response.AccessToken;
                await settingService.SetAsync(SettingRegistry.SearchEngineSpotifyAccessToken, apiAccessToken, cancellationToken);
            }

            if (searchResult?.Albums?.Items?.Count > 0)
            {
                var na = query.Artist.ToNormalizedString();
                foreach (var sr in searchResult.Albums.Items)
                {
                    var artist = sr.Artists.FirstOrDefault(x => x.Name.ToNormalizedString() == na);
                    if (artist != null)
                    {
                        short rank = 10;
                        if (sr.ReleaseDate == query.Year.ToString())
                        {
                            rank = 20;
                        }

                        var biggestImageHeight = sr.Images.Max(x => x.Height);
                        var image = sr.Images.FirstOrDefault(x => x.Height == biggestImageHeight);
                        if (image != null)
                        {
                            results.Add(new ImageSearchResult
                            {
                                SpotifyId = sr.Id,
                                ArtistSpotifyId = artist.Id,
                                FromPlugin = DisplayName,
                                Height = image.Height,
                                MediaUrl = image.Url,
                                Rank = rank,
                                ThumbnailUrl = image.Url,
                                Title = sr.Name,
                                UniqueId = SafeParser.Hash(sr.Id),
                                Width = image.Width
                            });
                        }
                    }
                }

                if (results.Count > 0)
                {
                    logger.Debug("[{DisplayName}] found [{ImageCount}] for Album [{Query}]",
                        DisplayName,
                        results.Count,
                        query.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error searching for album query [{Query}]", query.ToString());
        }

        return new OperationResult<ImageSearchResult[]?>
        {
            Data = results.OrderBy(x => x.Rank).ToArray()
        };
    }

    public async Task<OperationResult<ImageSearchResult[]?>> DoArtistImageSearch(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        var results = new List<ImageSearchResult>();

        try
        {
            var apiClientId = configuration.GetValue<string>(SettingRegistry.SearchEngineSpotifyApiKey);
            var apiClientSecret = configuration.GetValue<string>(SettingRegistry.SearchEngineSpotifyClientSecret);

            if (string.IsNullOrWhiteSpace(apiClientId) || string.IsNullOrWhiteSpace(apiClientSecret))
            {
                return new OperationResult<ImageSearchResult[]?>("Spotify API key not configured.")
                {
                    Data = []
                };
            }

            var apiAccessToken = configuration.GetValue<string>(SettingRegistry.SearchEngineSpotifyAccessToken);

            var config = SpotifyClientConfig.CreateDefault();

            if (string.IsNullOrWhiteSpace(apiAccessToken))
            {
                var request = new ClientCredentialsRequest(apiClientId, apiClientSecret);
                var response = await new OAuthClient(config).RequestToken(request, cancellationToken);
                apiAccessToken = response.AccessToken;
                await settingService.SetAsync(SettingRegistry.SearchEngineSpotifyAccessToken, apiAccessToken, cancellationToken);
            }

            var spotify = new SpotifyClient(config.WithToken(apiAccessToken));
            SearchResponse? searchResult = null;
            try
            {
                searchResult = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Artist, query.Name), cancellationToken);
            }
            catch (APIUnauthorizedException)
            {
                var request = new ClientCredentialsRequest(apiClientId, apiClientSecret);
                var response = await new OAuthClient(config).RequestToken(request, cancellationToken);
                apiAccessToken = response.AccessToken;
                await settingService.SetAsync(SettingRegistry.SearchEngineSpotifyAccessToken, apiAccessToken, cancellationToken);
            }

            if (searchResult?.Artists?.Items?.Count > 0)
            {
                foreach (var sr in searchResult.Artists.Items)
                {
                    var biggestImageHeight = sr.Images?.Max(x => x.Height) ?? 0;

                    short rank = 10;

                    var image = sr.Images?.FirstOrDefault(x => x.Height == biggestImageHeight);
                    if (image != null)
                    {
                        results.Add(new ImageSearchResult
                        {
                            SpotifyId = sr.Id,
                            ArtistSpotifyId = sr.Id,
                            FromPlugin = DisplayName,
                            Height = image.Height,
                            MediaUrl = image.Url,
                            Rank = rank,
                            ThumbnailUrl = image.Url,
                            Title = sr.Name,
                            UniqueId = SafeParser.Hash(sr.Id),
                            Width = image.Width
                        });
                    }
                }
            }

            if (results.Count > 0)
            {
                logger.Debug("[{DisplayName}] found [{ImageCount}] for Artist [{Query}]",
                    DisplayName,
                    results.Count,
                    query.ToString());
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error searching for artist query [{Query}]", query.ToString());
        }

        return new OperationResult<ImageSearchResult[]?>
        {
            Data = results.OrderBy(x => x.Rank).ToArray()
        };
    }

    public bool StopProcessing { get; } = false;

    public string Id => "BBAC49B7-0EDF-4D31-8A54-C9126509C2CE";

    public string DisplayName => "Spotify Service";

    public bool IsEnabled { get; set; } = false;

    public int SortOrder { get; } = 1;

    public async Task<PagedResult<ArtistSearchResult>> DoArtistSearchAsync(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        var results = new List<ArtistSearchResult>();

        try
        {
            var apiClientId = configuration.GetValue<string>(SettingRegistry.SearchEngineSpotifyApiKey);
            var apiClientSecret = configuration.GetValue<string>(SettingRegistry.SearchEngineSpotifyClientSecret);

            if (string.IsNullOrWhiteSpace(apiClientId) || string.IsNullOrWhiteSpace(apiClientSecret))
            {
                return new PagedResult<ArtistSearchResult>(["Spotify API key not configured."])
                {
                    Data = []
                };
            }

            var apiAccessToken = configuration.GetValue<string>(SettingRegistry.SearchEngineSpotifyAccessToken);

            var config = SpotifyClientConfig.CreateDefault(); //.WithRetryHandler(new MelodeeRetryHandler());

            if (string.IsNullOrWhiteSpace(apiAccessToken))
            {
                var request = new ClientCredentialsRequest(apiClientId, apiClientSecret);
                var response = await new OAuthClient(config).RequestToken(request, cancellationToken);
                apiAccessToken = response.AccessToken;
                await settingService.SetAsync(SettingRegistry.SearchEngineSpotifyAccessToken, apiAccessToken, cancellationToken);
            }

            var spotify = new SpotifyClient(config.WithToken(apiAccessToken));
            SearchResponse? searchResult = null;
            try
            {
                searchResult = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Artist, query.Name), cancellationToken);
            }
            catch (APIUnauthorizedException)
            {
                var request = new ClientCredentialsRequest(apiClientId, apiClientSecret);
                var response = await new OAuthClient(config).RequestToken(request, cancellationToken);
                apiAccessToken = response.AccessToken;
                await settingService.SetAsync(SettingRegistry.SearchEngineSpotifyAccessToken, apiAccessToken, cancellationToken);
            }

            if (searchResult?.Artists?.Items?.Count > 0)
            {
                foreach (var sr in searchResult.Artists.Items)
                {
                    results.Add(new ArtistSearchResult
                    {
                        Name = sr.Name,
                        FromPlugin = DisplayName,
                        AlbumCount = null,
                        UniqueId = SafeParser.Hash(sr.Id),
                        Rank = 1,
                        ImageUrl = sr.Images.FirstOrDefault()?.Url,
                        ThumbnailUrl = sr.Images.LastOrDefault()?.Url,
                        SpotifyId = sr.Id
                    });
                    if (results.Count >= maxResults)
                    {
                        break;
                    }
                }

                if (results.Count > 0)
                {
                    var newResults = new List<ArtistSearchResult>();
                    foreach (var artist in results.Where(x => x.SpotifyId != null))
                    {
                        var artistAlbumsResult = await spotify.Artists.GetAlbums(artist.SpotifyId!, new ArtistsAlbumsRequest(), cancellationToken);
                        if (artistAlbumsResult?.Items?.Count > 0)
                        {
                            newResults.Add(artist with
                            {
                                Releases = artistAlbumsResult.Items.Select(x => new AlbumSearchResult
                                {
                                    AlbumType = x.AlbumType.ToNormalizedString() == "ALBUM" ? AlbumType.Album : AlbumType.NotSet,
                                    Name = x.Name,
                                    NameNormalized = x.Name.ToNormalizedString() ?? x.Name,
                                    ReleaseDate = x.ReleaseDate,
                                    SortName = x.Name,
                                    SpotifyId = x.Id
                                }).ToArray()
                            });
                        }
                        var matchingAlbumCount = query.AlbumKeyValues == null ? 0 : (artistAlbumsResult?.Items ?? []).Count(x => query.AlbumKeyValues?.Any(y => y.Key == x.ReleaseDate[..4] && y.Value == x.Name.ToNormalizedString()) ?? false);
                        artist.Rank += matchingAlbumCount;
                    }
                    if (newResults.Count > 0)
                    {
                        results = newResults;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error searching for artist query [{Query}]", query.ToString());
        }

        return new PagedResult<ArtistSearchResult>
        {
            Data = results.OrderBy(x => x.Rank).ToArray()
        };
    }

    public Task<PagedResult<SongSearchResult>> DoArtistTopSongsSearchAsync(int forArtist, int maxResults, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PagedResult<SongSearchResult>(["Spotify Not implemented"]) { Data = [] });
    }
}

public class MelodeeRetryHandler : IRetryHandler
{
    public Task<IResponse> HandleRetry(IRequest request, IResponse response, IRetryHandler.RetryFunc retry, CancellationToken cancel = default)
    {
        Thread.Sleep(1000);
        var newResponse = retry(request, cancel);
        return newResponse;
    }
}
