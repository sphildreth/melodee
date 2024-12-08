using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Plugins.SearchEngine.MusicBrainz.CoverArtArchive.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Serilog;

namespace Melodee.Plugins.SearchEngine.MusicBrainz;

/// <summary>
///     https://musicbrainz.org/doc/Cover_Art_Archive/API
/// </summary>
public sealed class MusicBrainzCoverArtArchiveSearchEngine(IMelodeeConfiguration configuration, ISerializer serializer, IHttpClientFactory httpClientFactory) : IAlbumImageSearchEnginePlugin
{
    public bool StopProcessing { get; } = false;

    public string Id => "3E6C2DD3-AC1A-452D-B52B-4C292BA1CC49";

    public string DisplayName => nameof(MusicBrainzCoverArtArchiveSearchEngine);

    public bool IsEnabled { get; set; }

    public int SortOrder { get; } = 0;

    public async Task<OperationResult<ImageSearchResult[]?>> DoSearch(AlbumQuery query, int maxResults, CancellationToken token = default)
    {
        var result = new List<ImageSearchResult>();

        if (!configuration.GetValue<bool>(SettingRegistry.SearchEngineMusicBrainzEnabled))
        {
            return new OperationResult<ImageSearchResult[]?>("MusicBrainz Cover Art Archive image search plugin is disabled.")
            {
                Data = null
            };
        }

        try
        {
            if (query.MusicBrainzIdValue != null)
            {
                var httpRequestMessage = new HttpRequestMessage(
                    HttpMethod.Get,
                    new Uri($"http://coverartarchive.org/release/{ query.MusicBrainzId}"))
                {
                    Headers =
                    {
                        { HeaderNames.Accept, "application/json" },
                        { HeaderNames.UserAgent, configuration.GetValue<string?>(SettingRegistry.SearchEngineUserAgent) },
                    }
                };

                var httpClient = httpClientFactory.CreateClient();

                var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, token);

                var contentString = await httpResponseMessage.Content.ReadAsStringAsync(token);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var searchResponse = serializer.Deserialize<ReleaseInfoResult?>(contentString);             
                    if (searchResponse != null)
                    {
                        result.AddRange(searchResponse?.Images.Select(x => new ImageSearchResult
                        {
                            FromPlugin = nameof(MusicBrainzCoverArtArchiveSearchEngine),
                            Rank = 1,
                            ThumbnailUrl = x.Thumbnails.OrderBy(x => SafeParser.ToNumber<int>(x.Key)).FirstOrDefault().Value,
                            MediaUrl = x.Image,
                            Title = x.Comment
                        }) ?? []);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "[{PluginName}] attempting to query cover art archive image search plugin failed Query [{Query}]", nameof(MusicBrainzArtistSearchEnginPlugin), query);
        }

        if (result.Count == 0)
        {
            Log.Debug("[{PluginName}] no MusicBrainz Cover Art response for query[{Query}]", nameof(MusicBrainzArtistSearchEnginPlugin), query);
        }
        
        return new OperationResult<ImageSearchResult[]?>
        {
            Data = result.ToArray()
        };
    }
}
