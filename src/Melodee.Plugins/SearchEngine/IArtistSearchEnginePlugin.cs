using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;

namespace Melodee.Plugins.SearchEngine;

public interface IArtistSearchEngine : IPlugin
{
    Task<OperationResult<ArtistSearchResult[]>> PerformArtistSearchAsync(string query, int resultsCount);
}
