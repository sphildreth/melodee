using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.ITunes;

namespace Melodee.Tests.Plugins.SearchEngine;

public class ITunesTests : TestsBase
{
    [Fact]
    public async Task PerformITunesAlbumSearch()
    {
        using (var httpClient = new HttpClient())
        {
            var itunes = new ITunesSearchEngine(Logger, Serializer, new TestHttpClientFactory(httpClient));
            var result = await itunes.DoAlbumImageSearch(new AlbumQuery
            {
                Year = 1983,
                Name = "Cargo",
                Artist = "Men At Work"
            }, 10);
            Assert.NotNull(result);
        }
    }
}
