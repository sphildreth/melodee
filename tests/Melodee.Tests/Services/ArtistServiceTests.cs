using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using NodaTime;
using Artist = Melodee.Common.Data.Models.Artist;

namespace Melodee.Tests.Services;

public class ArtistServiceTests : ServiceTestBase
{
    [Fact]
    public async Task ListArtistsAsync()
    {
        var shouldContainApiKey = Guid.NewGuid();

        var artistName = "Bob Jones";
        var artist = new Melodee.Common.Models.Artist(artistName, artistName.ToNormalizedString()!, artistName.CleanString(doPutTheAtEnd: true));
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Artists.Add(new Artist
            {
                ApiKey = shouldContainApiKey,
                Directory = artist.ToDirectoryName(255),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = 1,
                Name = "Bob Jones",
                NameNormalized = "Bob Jones".ToNormalizedString()!
            });
            await context.SaveChangesAsync();
        }

        var listResult = await GetArtistService().ListAsync(new PagedRequest());
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.ApiKey == shouldContainApiKey);
        Assert.Equal(1, listResult.TotalPages);
        Assert.Equal(1, listResult.TotalCount);
    }
 

    [Fact]
    public async Task GetByNameNormalizedAsync()
    {
        var artistName = "Bob Jones";
        var artist = new Melodee.Common.Models.Artist(artistName, artistName.ToNormalizedString()!, artistName.CleanString(doPutTheAtEnd: true));

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Artists.Add(new Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = artist.ToDirectoryName(255),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = 1,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!
            });
            await context.SaveChangesAsync();
        }

        var result = await GetArtistService().GetByNameNormalized(artistName.ToNormalizedString()!);
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(artistName, result.Data.Name);
    }
}
