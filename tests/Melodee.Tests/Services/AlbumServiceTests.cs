using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Tests.Validation;
using NodaTime;
using Album = Melodee.Common.Data.Models.Album;
using Artist = Melodee.Common.Data.Models.Artist;

namespace Melodee.Tests.Services;

public class AlbumServiceTests : ServiceTestBase
{
    [Fact]
    public async Task ListAlbumsAsync()
    {
        var shouldContainApiKey = Guid.NewGuid();

        var artistName = "Bob Jones";
        var melodeeArtist = new Melodee.Common.Models.Artist(artistName, artistName.ToNormalizedString()!, artistName.CleanString(true), null, 1);
        var melodeeAlbum = AlbumValidatorTests.TestAlbum;

        var configuration = TestsBase.NewPluginsConfiguration();

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var artist = new Artist
            {
                ApiKey = shouldContainApiKey,
                Directory = melodeeArtist.ToDirectoryName(255),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = 1,
                Name = "Bob Jones",
                NameNormalized = "Bob Jones".ToNormalizedString()!
            };
            context.Artists.Add(artist);
            context.Albums.Add(new Album
            {
                Artist = artist,
                ApiKey = shouldContainApiKey,
                Directory = melodeeAlbum.ToDirectoryName(),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                Name = melodeeAlbum.AlbumTitle()!,
                NameNormalized = melodeeAlbum.AlbumTitle().ToNormalizedString()!,
                AlbumStatus = (short)melodeeAlbum.Status,
                AlbumType = (int)AlbumType.Album,
                DiscCount = melodeeAlbum.MediaCountValue(),
                Duration = melodeeAlbum.TotalDuration(),
                Genres = melodeeAlbum.Genre() == null ? null : melodeeAlbum.Genre()!.Split('/'),
                IsCompilation = melodeeAlbum.IsVariousArtistTypeAlbum(),
                MusicBrainzId = SafeParser.ToGuid(melodeeAlbum.MusicBrainzId),
                MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                OriginalReleaseDate = melodeeAlbum.OriginalAlbumYear() == null ? null : SafeParser.ToLocalDate(melodeeAlbum.OriginalAlbumYear()!.Value),
                ReleaseDate = SafeParser.ToLocalDate(melodeeAlbum.AlbumYear() ?? throw new Exception("Album year is required.")),
                SongCount = SafeParser.ToNumber<short>(melodeeAlbum.Songs?.Count() ?? 0),
                SortName = configuration.RemoveUnwantedArticles(melodeeAlbum.AlbumTitle().CleanString(true))
            });

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        var listResult = await GetAlbumService().ListAsync(new PagedRequest());
        AssertResultIsSuccessful(listResult);
        Assert.Contains(listResult.Data, x => x.ApiKey == shouldContainApiKey);
        Assert.Equal(1, listResult.TotalPages);
        Assert.Equal(1, listResult.TotalCount);
    }


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
