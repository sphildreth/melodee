using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;
using Serilog;

namespace Melodee.Common.Plugins.SearchEngine.Deezer;

/// <summary>
/// Search engine using the Deezer API
/// <remarks>
/// https://developers.deezer.com/api
/// </remarks>
/// </summary>
public class DeezerSearchEngine(
    ILogger logger,
    ISerializer serializer,
    IHttpClientFactory httpClientFactory)
    : IAlbumImageSearchEnginePlugin, IArtistImageSearchEnginePlugin
{
    private static readonly int Width = 1000;
    private static readonly int Height = 1000;    
    
    
    public bool StopProcessing { get; } = false;

    public string Id => "A1528971-44B3-42A7-B658-DB96ABCEF10D";

    public string DisplayName => "Deezer Search Engine";

    public bool IsEnabled { get; set; }

    public int SortOrder { get; } = 2;
    
    public async Task<OperationResult<ImageSearchResult[]?>> DoArtistImageSearch(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        // https://api.deezer.com/search/artist?q=%22Billy%20Joel%22&output=json&order=Ranking&limit=1
        
        var results = new List<ImageSearchResult>();

        if (string.IsNullOrWhiteSpace(query.Name))
        {
            return new OperationResult<ImageSearchResult[]?>
            {
                Data = []
            };
        }

        var httpClient = httpClientFactory.CreateClient();
        var requestUri = $"https://api.deezer.com/search/artist?q=\"{Uri.EscapeDataString(query.Name.Trim())}\"&output=json&limit=1&order=RANKING";        

        try
        {
            var response = await httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new OperationResult<ImageSearchResult[]?>
                {
                    Data = []
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            var searchResult = serializer.Deserialize<ArtistSearchResult>(jsonResponse);
            var data = searchResult?.Data.FirstOrDefault();            
            if (data != null)
            {
                results.Add(new ImageSearchResult
                {
                    DeezerId = data.Id,
                    FromPlugin = DisplayName,
                    Height = Height,
                    MediaUrl = data.Picture_Xl,
                    Rank = 1,
                    ThumbnailUrl = data.Picture_Small,
                    Title = data.Name,
                    UniqueId = data.Id,
                    Width = Width
                });

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
            logger.Error(ex, "Error searching for album image url [{Url}] query [{Query}]", requestUri, query.ToString());
        }

        return new OperationResult<ImageSearchResult[]?>
        {
            Data = results.OrderBy(x => x.Rank).ToArray()
        };
    }
    
    public async Task<OperationResult<ImageSearchResult[]?>> DoAlbumImageSearch(AlbumQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        //https://api.deezer.com/search/album?q=%22Cold%20Spring%20Harbor%22&output=json&limit=1
        
        var results = new List<ImageSearchResult>();

        if (string.IsNullOrWhiteSpace(query.Name))
        {
            return new OperationResult<ImageSearchResult[]?>
            {
                Data = []
            };
        }

        var httpClient = httpClientFactory.CreateClient();
        var requestUri = $"https://api.deezer.com/search/album?q=\"{Uri.EscapeDataString(query.Name.Trim())}\"&output=json&limit={maxResults}";

        try
        {
            var response = await httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new OperationResult<ImageSearchResult[]?>
                {
                    Data = []
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            var searchResult = serializer.Deserialize<AlbumSearchResult>(jsonResponse) ?? new AlbumSearchResult([], 0);
            if (searchResult.Data.Length != 0)
            {
                var na = query.Artist.ToNormalizedString();
                foreach (var sr in searchResult.Data.Where(x => x is { Cover_Small : not null, Cover_Xl: not null }))
                {
                    if (sr.Artist?.Name.ToNormalizedString() == na)
                    {
                        results.Add(new ImageSearchResult
                        {
                            DeezerId = sr.Id,
                            FromPlugin = DisplayName,
                            Height = Height,
                            MediaUrl = sr.Cover_Xl,
                            Rank = 10,
                            ThumbnailUrl = sr.Cover_Small,
                            Title = sr.Title,
                            UniqueId = sr.Id,
                            Width = Width
                        });
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
            logger.Error(ex, "Error searching for album image url [{Url}] query [{Query}]", requestUri, query.ToString());
        }

        return new OperationResult<ImageSearchResult[]?>
        {
            Data = results.OrderBy(x => x.Rank).ToArray()
        };
    }    
}
