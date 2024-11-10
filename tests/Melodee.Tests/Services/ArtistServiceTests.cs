using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using NodaTime;

namespace Melodee.Tests.Services;

public class ArtistServiceTests : ServiceTestBase
{
    [Fact]
    public async Task ListArtistsAsync()
    {
        var shouldContainApiKey = Guid.NewGuid();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Artists.Add(new Artist
            {
                ApiKey = shouldContainApiKey,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
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
    public async Task GetByMediaUniqueId()
    {
        long shouldByMediaUniqueId = 12345;

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Artists.Add(new Artist
            {
                ApiKey = Guid.NewGuid(),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                Name = "Bob Jones",
                NameNormalized = "Bob Jones".ToNormalizedString()!,
                MediaUniqueId = shouldByMediaUniqueId
            });
            context.Artists.Add(new Artist
            {
                ApiKey = Guid.NewGuid(),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                Name = "Grace Jones",
                NameNormalized = "Grace Jones".ToNormalizedString()!,
                MediaUniqueId = shouldByMediaUniqueId + 1
            });            
            await context.SaveChangesAsync();
        }

        var result = await GetArtistService().GetByMediaUniqueId(shouldByMediaUniqueId);
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(shouldByMediaUniqueId, result.Data.MediaUniqueId);
    }    

    [Fact]
    public async Task GetByNameNormalizedAsync()
    {
        var name = "Bob Jones";

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            context.Artists.Add(new Artist
            {
                ApiKey = Guid.NewGuid(),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                Name = name,
                NameNormalized = name.ToNormalizedString()!
            });
            await context.SaveChangesAsync();
        }

        var result = await GetArtistService().GetByNameNormalized(name.ToNormalizedString()!);
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(name, result.Data.Name);
    }
}
