using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using System.Text.Json.Nodes;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Melodee.Plugins.SearchEngine;

/// <summary>
/// Bing image search plugin.
/// <remarks>https://learn.microsoft.com/en-us/bing/search-apis/bing-image-search/quickstarts/rest/csharp</remarks>
/// </summary>
public sealed class BingImageSearchEngine(IMelodeeConfiguration configuration, ISerializer serializer, IHttpClientFactory httpClientFactory) : IImageSearchEnginePlugin
{
    public string Id => "7E8863EE-E95F-42F8-A4DE-693D78DC216C";

    public string DisplayName => nameof(BingImageSearchEngine);

    public bool IsEnabled { get; set; } = false;

    public int SortOrder { get; } = 0;

    public bool StopProcessing { get; } = false;
    
    public async Task<OperationResult<ImageSearchResult[]?>> DoSearch(string query, int maxResults, CancellationToken token = default)
    {
        var result =  new List<ImageSearchResult>();
        
        var bingApiKey = configuration.GetValue<string?>(SettingRegistry.SearchEngineApiKeyBingImage);
        if (bingApiKey.Nullify() == null || !configuration.GetValue<bool>(SettingRegistry.SearchEngineEnabledBingImage))
        {
            return new OperationResult<ImageSearchResult[]?>("Bing image search plugin is disabled.")
            {
                Data = null
            };
        }
        
        var queryArguments = new Dictionary<string, string?>()
        {
            { "count", maxResults.ToString() },
            { "safeSearch", "Off" },
            { "aspect", "Square" },
            { "q", $"'{Uri.EscapeDataString(query.Trim())}'" },
        };
        
        var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            QueryHelpers.AddQueryString("https://api.bing.microsoft.com/v7.0/images/search", queryArguments))
        {
            Headers =
            {
                { HeaderNames.Accept, "application/json" },
                { HeaderNames.UserAgent, configuration.GetValue<string?>(SettingRegistry.SearchEngineUserAgent) },
                { "Ocp-Apim-Subscription-Key", bingApiKey }
            }
        };
        
        var httpClient = httpClientFactory.CreateClient();
        
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, token);

        var contentString = await httpResponseMessage.Content.ReadAsStringAsync(token);
        Dictionary<string, object?> searchResponse = serializer.Deserialize<Dictionary<string, object>>(contentString) ?? new Dictionary<string, object?>();
        
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var imageResult = serializer.Deserialize<BingImageSearchResult[]>(searchResponse["value"]?.ToString());
            
            if (imageResult?.Any() ?? false)
            {
                result.AddRange(imageResult.Select(x => new ImageSearchResult(
                    SafeParser.Hash(x.thumbnailUrl ?? string.Empty, x.contentUrl ?? string.Empty),
                    x.width ?? 0,
                    x.height ?? 0,
                    x.thumbnailUrl ?? string.Empty,
                    x.contentUrl ?? string.Empty,
                    x.hostPageUrl ?? string.Empty)));
            }
        }
        else
        {
            if (searchResponse.TryGetValue("error", out object? value))  // typically 401, 403
            {
                 if (value is HttpStatusCode.Unauthorized)
                 {
                     throw new AuthenticationException("Bing ApiKey is not correct");
                 }
                 if (value != null)
                 {
                     throw new Exception($"Bin request error: {value}.");
                 }             
            }
            else if (searchResponse.TryGetValue("errors", out value))
            {
                return new OperationResult<ImageSearchResult[]?>(searchResponse["errors"] as string ?? string.Empty)
                {
                    Data = null
                };                
            }            
        }
        return new OperationResult<ImageSearchResult[]?>
        {
            Data = result.ToArray()
        };
    }
}

public record BingImageSearchResult(
    string? webSearchUrl,
    string? name,
    string? thumbnailUrl,
    string? datePublished,
    bool? isFamilyFriendly,
    string? contentUrl,
    string? hostPageUrl,
    string? contentSize,
    string? encodingFormat,
    string? hostPageDisplayUrl,
    int? width,
    int? height,
    string? hostPageDiscoveredDate,
    bool? isTransparent,
    BingThumbnail? thumbnail,
    string? imageInsightsToken,
    BingInsightsMetadata? insightsMetadata,
    string? imageId,
    string? accentColor
);

public record BingThumbnail(
    int? width,
    int? height
);

public record BingInsightsMetadata(
    int? pagesIncludingCount,
    int? availableSizesCount
);
