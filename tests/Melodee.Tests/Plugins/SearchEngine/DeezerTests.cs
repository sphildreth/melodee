using Melodee.Common.Plugins.SearchEngine.Deezer;

namespace Melodee.Tests.Plugins.SearchEngine;

public class DeezerTests : TestsBase
{
    [Fact]
    public void ValidateDeezerArtistResultDeserialization()
    {
        var json = """
                   {
                     "data" : [ {
                       "id" : 882,
                       "name" : "The Offspring",
                       "link" : "https://www.deezer.com/artist/882",
                       "picture" : "https://api.deezer.com/artist/882/image",
                       "picture_small" : "https://cdn-images.dzcdn.net/images/artist/cb39bd249507f7f66782834195be99de/56x56-000000-80-0-0.jpg",
                       "picture_medium" : "https://cdn-images.dzcdn.net/images/artist/cb39bd249507f7f66782834195be99de/250x250-000000-80-0-0.jpg",
                       "picture_big" : "https://cdn-images.dzcdn.net/images/artist/cb39bd249507f7f66782834195be99de/500x500-000000-80-0-0.jpg",
                       "picture_xl" : "https://cdn-images.dzcdn.net/images/artist/cb39bd249507f7f66782834195be99de/1000x1000-000000-80-0-0.jpg",
                       "nb_album" : 28,
                       "nb_fan" : 1836147,
                       "radio" : true,
                       "tracklist" : "https://api.deezer.com/artist/882/top?limit=50",
                       "type" : "artist"
                     } ],
                     "total" : 11,
                     "next" : "https://api.deezer.com/search/artist?q=%22The%20Offspring%22&output=json&limit=1&order=RANKING&index=1"
                   }
                   """;
        var artistResult = Serializer.Deserialize<ArtistSearchResult>(json);
        Assert.NotNull(artistResult);
        Assert.NotNull(artistResult.Data);
        Assert.NotNull(artistResult.Data.First());
        Assert.Equal("The Offspring", artistResult.Data.First().Name);
        Assert.Equal(11, artistResult.Total);
        Assert.Equal("https://cdn-images.dzcdn.net/images/artist/cb39bd249507f7f66782834195be99de/1000x1000-000000-80-0-0.jpg", artistResult.Data.First().Picture_Xl);
    }
}
