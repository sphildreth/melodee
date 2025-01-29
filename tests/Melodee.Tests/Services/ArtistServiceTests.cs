using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;
using NodaTime;
using Artist = Melodee.Common.Data.Models.Artist;

namespace Melodee.Tests.Services;

public class ArtistServiceTests : ServiceTestBase
{
    [Fact]
    public async Task GetByNameNormalizedAsync()
    {
        var artistName = "Bob Jones";
        var artist = new Melodee.Common.Models.Artist(artistName, artistName.ToNormalizedString()!, artistName.CleanString(true), null, 1);

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
