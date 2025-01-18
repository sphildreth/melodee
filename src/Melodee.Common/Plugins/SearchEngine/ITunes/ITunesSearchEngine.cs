using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;
using Serilog;

namespace Melodee.Common.Plugins.SearchEngine.ITunes;

/// <summary>
/// Search apple itunes using the api
/// <remarks>
/// https://performance-partners.apple.com/search-api
/// </remarks>
/// </summary>
public class ITunesSearchEngine(
    ILogger logger,
    ISerializer serializer,
    IHttpClientFactory httpClientFactory)
    : IAlbumImageSearchEnginePlugin, IArtistSearchEnginePlugin
{
    public bool StopProcessing { get; } = false;

    public string Id => "36E4DB7A-18BB-4A7D-86F7-A3C653A21611";

    public string DisplayName => "ITunes Search Engine";

    public bool IsEnabled { get; set; }

    public int SortOrder { get; } = 2;
    
    public async Task<OperationResult<ImageSearchResult[]?>> DoAlbumImageSearch(AlbumQuery query, int maxResults, CancellationToken token = default)
    {
        //https://itunes.apple.com/search?term=Cargo&entity=album&country=US&media=music

        var results = new List<ImageSearchResult>();
        
        if (string.IsNullOrWhiteSpace(query.Name))
        {
            return new OperationResult<ImageSearchResult[]?>
            {
                Data = []
            };
        }

        var httpClient = httpClientFactory.CreateClient();
        var requestUri =$"https://itunes.apple.com/search?term={Uri.EscapeDataString(query.Name.Trim())}&entity=album&country={query.Country}&media=music&limit={maxResults}";

        try
        {
            var response = await httpClient.GetAsync(requestUri, token);

            if (!response.IsSuccessStatusCode)
            {
                return new OperationResult<ImageSearchResult[]?>
                {
                    Data = []
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(token);
            var searchResult = serializer.Deserialize<ITunesSearchResult>(jsonResponse);
            if (searchResult?.Results?.Any() ?? false)
            {
                var na = query.Artist.ToNormalizedString();
                foreach (var sr in searchResult.Results.Where(x => x is { ArtworkUrl100: not null, ArtworkUrl60: not null }))
                {
                    if (sr.ArtistName.ToNormalizedString() == na)
                    {
                        short rank = 10;
                        if (sr.ReleaseDate?.Year == query.Year)
                        {
                            rank = 20;
                        }
                        var mediaUrl = sr.ArtworkUrl100!.Replace("100x100bb", "600x600bb");
                        results.Add(new ImageSearchResult
                        {
                            ArtistAmgId = sr.AmgArtistId?.ToString(),
                            ArtistItunesId = sr.ArtistId?.ToString(),
                            FromPlugin = DisplayName,
                            Height = 600,
                            ItunesId = sr.CollectionId?.ToString(),
                            MediaUrl = mediaUrl,
                            Rank = rank,
                            ThumbnailUrl = sr.ArtworkUrl100!,
                            Title = sr.CollectionName,
                            UniqueId = sr.CollectionId ?? 0 + sr.ArtistId ?? 0,
                            Width = 600
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error searching for album image url [{Url}] query [{Query}]", requestUri, query.Name);
        }
        return new OperationResult<ImageSearchResult[]?>
        {
            Data = results.OrderBy(x => x.Rank).ToArray()
        };
    }

    public Task<PagedResult<ArtistSearchResult>> DoArtistSearchAsync(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        //https://itunes.apple.com/search?term=Colin%2BHay&entity=musicArtist&country=US&media=music
        
        
        // try
        // {
        //     var request = BuildRequest(query, 1, "musicArtist");
        //     var response = await _client.ExecuteAsync<ITunesSearchResult>(request).ConfigureAwait(false);
        //     if (response.ResponseStatus == ResponseStatus.Error)
        //     {
        //         if (response.StatusCode == HttpStatusCode.Unauthorized)
        //         {
        //             throw new AuthenticationException("Unauthorized");
        //         }
        //         throw new Exception(string.Format("Request Error Message: {0}. Content: {1}.", response.ErrorMessage, response.Content));
        //     }
        //
        //     var responseData = response?.Data?.results?.FirstOrDefault();
        //     if (responseData != null)
        //     {
        //         var urls = new List<string>();
        //         if (!string.IsNullOrEmpty(responseData.artistLinkUrl)) urls.Add(responseData.artistLinkUrl);
        //         if (!string.IsNullOrEmpty(responseData.artistViewUrl)) urls.Add(responseData.artistViewUrl);
        //         if (!string.IsNullOrEmpty(responseData.collectionViewUrl)) urls.Add(responseData.collectionViewUrl);
        //         data = new ArtistSearchResult
        //         {
        //             ArtistName = responseData.artistName,
        //             iTunesId = responseData.artistId.ToString(),
        //             AmgId = responseData.amgArtistId.ToString(),
        //             ArtistType = responseData.artistType,
        //             ArtistThumbnailUrl = responseData.artworkUrl100,
        //             ArtistGenres = new[] { responseData.primaryGenreName },
        //             Urls = urls
        //         };
        //     }
        // }
        // catch (Exception ex)
        // {
        //     Logger.LogError(ex);
        // }
        
        throw new NotImplementedException();
    }
    
    // private RestRequest BuildRequest(string query, int resultsCount, string entityType)
    // {
    //     var request = new RestRequest
    //     {
    //         Resource = "search",
    //         Method = Method.Get,
    //         RequestFormat = DataFormat.Json
    //     };
    //
    //     if (resultsCount > 0)
    //     {
    //         request.AddParameter("limit", resultsCount, ParameterType.GetOrPost);
    //     }
    //     request.AddParameter("entity", entityType, ParameterType.GetOrPost);
    //     request.AddParameter("country", "us", ParameterType.GetOrPost);
    //     request.AddParameter("term", string.Format("'{0}'", query.Trim()), ParameterType.GetOrPost);
    //     return request;
    // }    
}
