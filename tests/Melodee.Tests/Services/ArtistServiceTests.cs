using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.Extensions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;
using Artist = Melodee.Common.Data.Models.Artist;
using Library = Melodee.Common.Data.Models.Library;

namespace Melodee.Tests.Services;

public class ArtistServiceTests : ServiceTestBase
{
    #region GetByNameNormalized Tests

    [Fact]
    public async Task GetByNameNormalizedAsync_ValidName_ReturnsArtist()
    {
        // Arrange
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

        // Act
        var result = await GetArtistService().GetByNameNormalized(artistName.ToNormalizedString()!);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(artistName, result.Data.Name);
    }

    [Fact]
    public async Task GetByNameNormalizedAsync_NonExistentName_ReturnsError()
    {
        // Arrange
        var nonExistentName = "NonExistentArtist".ToNormalizedString()!;

        // Act
        var result = await GetArtistService().GetByNameNormalized(nonExistentName);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown artist", result.Messages?.FirstOrDefault() ?? string.Empty);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetByNameNormalizedAsync_NullOrEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetByNameNormalized(string.Empty));
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetByNameNormalized(null!));
    }

    #endregion

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_ValidId_ReturnsArtist()
    {
        // Arrange
        var artistName = "Test Artist";
        var artistId = 0;

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = "test-artist",
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!,
                Library = library
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();
            artistId = artist.Id;
        }

        // Act
        var result = await GetArtistService().GetAsync(artistId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(artistName, result.Data.Name);
        Assert.NotNull(result.Data.Library);
    }

    [Fact]
    public async Task GetAsync_InvalidId_ReturnsNull()
    {
        // Act
        var result = await GetArtistService().GetAsync(999999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetAsync_ZeroOrNegativeId_ThrowsArgumentException()
    {
        // Arrange
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAsync(0));
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAsync(-1));
    }

    #endregion

    #region GetByApiKeyAsync Tests

    [Fact]
    public async Task GetByApiKeyAsync_ValidApiKey_ReturnsArtist()
    {
        // Arrange
        var apiKey = Guid.NewGuid();
        var artistName = "API Test Artist";

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new Artist
            {
                ApiKey = apiKey,
                Directory = "api-test-artist",
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!,
                Library = library
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await GetArtistService().GetByApiKeyAsync(apiKey);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(artistName, result.Data.Name);
        Assert.Equal(apiKey, result.Data.ApiKey);
    }

    [Fact]
    public async Task GetByApiKeyAsync_NonExistentApiKey_ReturnsError()
    {
        // Arrange
        var nonExistentApiKey = Guid.NewGuid();

        // Act
        var result = await GetArtistService().GetByApiKeyAsync(nonExistentApiKey);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown artist", result.Messages?.FirstOrDefault() ?? string.Empty);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetByApiKeyAsync_EmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetByApiKeyAsync(Guid.Empty));
    }

    #endregion

    #region GetByMusicBrainzIdAsync Tests

    [Fact]
    public async Task GetByMusicBrainzIdAsync_ValidMusicBrainzId_ReturnsArtist()
    {
        // Arrange
        var musicBrainzId = Guid.NewGuid();
        var artistName = "MusicBrainz Test Artist";

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new Artist
            {
                ApiKey = Guid.NewGuid(),
                MusicBrainzId = musicBrainzId,
                Directory = "musicbrainz-test-artist",
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!,
                Library = library
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await GetArtistService().GetByMusicBrainzIdAsync(musicBrainzId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(artistName, result.Data.Name);
        Assert.Equal(musicBrainzId, result.Data.MusicBrainzId);
    }

    [Fact]
    public async Task GetByMusicBrainzIdAsync_NonExistentMusicBrainzId_ReturnsError()
    {
        // Arrange
        var nonExistentMusicBrainzId = Guid.NewGuid();

        // Act
        var result = await GetArtistService().GetByMusicBrainzIdAsync(nonExistentMusicBrainzId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown artist", result.Messages?.FirstOrDefault() ?? string.Empty);
        Assert.Null(result.Data);
    }

    #endregion

    #region FindArtistAsync Tests

    [Fact]
    public async Task FindArtistAsync_ByValidId_ReturnsArtist()
    {
        // Arrange
        var artistName = "Find Test Artist";
        var artistId = 0;

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = "find-test-artist",
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!,
                Library = library
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();
            artistId = artist.Id;
        }

        // Act
        var result = await GetArtistService().FindArtistAsync(artistId, Guid.Empty, null, null, null);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(artistName, result.Data.Name);
    }

    [Fact]
    public async Task FindArtistAsync_ByValidApiKey_ReturnsArtist()
    {
        // Arrange
        var apiKey = Guid.NewGuid();
        var artistName = "Find API Test Artist";

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new Artist
            {
                ApiKey = apiKey,
                Directory = "find-api-test-artist",
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!,
                Library = library
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await GetArtistService().FindArtistAsync(null, apiKey, null, null, null);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(artistName, result.Data.Name);
        Assert.Equal(apiKey, result.Data.ApiKey);
    }

    [Fact]
    public async Task FindArtistAsync_ByValidName_ReturnsArtist()
    {
        // Arrange
        var artistName = "Find Name Test Artist";
        var normalizedName = artistName.ToNormalizedString()!;

        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                Name = "Test Library",
                Path = "/test/path",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = "find-name-test-artist",
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = normalizedName,
                Library = library
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await GetArtistService().FindArtistAsync(null, Guid.Empty, normalizedName, null, null);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(artistName, result.Data.Name);
    }

    [Fact]
    public async Task FindArtistAsync_NoValidParameters_ReturnsError()
    {
        // Act
        var result = await GetArtistService().FindArtistAsync(null, Guid.Empty, null, null, null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown artist", result.Messages?.FirstOrDefault() ?? string.Empty);
        Assert.Null(result.Data);
    }

    #endregion

    #region ListAsync Tests

    [Fact]
    public async Task ListAsync_WithoutFilters_ReturnsAllArtists()
    {
        // Arrange
        await SeedTestArtists();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1
        };

        // Act
        var result = await GetArtistService().ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
        Assert.NotEmpty(result.Data);
        Assert.True(result.Data.Count() <= 10);
    }

    [Fact]
    public async Task ListAsync_WithNameFilter_ReturnsFilteredArtists()
    {
        // Arrange
        await SeedTestArtists();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            FilterBy = new[]
            {
                new FilterOperatorInfo("name", FilterOperator.Contains, "Test Artist 1")
            }
        };

        // Act
        var result = await GetArtistService().ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Data.All(a => a.Name.Contains("Test Artist 1")));
    }

    [Fact]
    public async Task ListAsync_WithPaging_ReturnsCorrectPage()
    {
        // Arrange
        await SeedTestArtists();
        var pagedRequest = new PagedRequest
        {
            PageSize = 2,
            Page = 2
        };

        // Act
        var result = await GetArtistService().ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Data.Count() <= 2);
    }

    [Fact]
    public async Task ListAsync_WithTotalCountOnlyRequest_ReturnsCountOnly()
    {
        // Arrange
        await SeedTestArtists();
        var pagedRequest = new PagedRequest
        {
            IsTotalCountOnlyRequest = true
        };

        // Act
        var result = await GetArtistService().ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task ListAsync_WithOrderBy_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestArtists();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "Name", "ASC" } }
        };

        // Act
        var result = await GetArtistService().ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
        
        // Verify ordering
        var names = result.Data.Select(a => a.Name).ToArray();
        var sortedNames = names.OrderBy(n => n).ToArray();
        Assert.Equal(sortedNames, names);
    }

    #endregion

    #region AddArtistAsync Tests

    [Fact]
    public async Task AddArtistAsync_ValidArtist_CreatesArtist()
    {
        // Arrange
        var library = await CreateTestLibrary();
        var artist = new Artist
        {
            Name = "New Test Artist",
            NameNormalized = "New Test Artist".ToNormalizedString()!,
            LibraryId = library.Id,
            Directory = "new-test-artist",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        // Act
        var result = await GetArtistService().AddArtistAsync(artist);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal("New Test Artist", result.Data.Name);
        Assert.NotEqual(Guid.Empty, result.Data.ApiKey);
        Assert.True(result.Data.CreatedAt > Instant.MinValue);
    }

    [Fact]
    public async Task AddArtistAsync_NullArtist_ThrowsArgumentNullException()
    {
        // Arrange
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddArtistAsync(null!));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ValidArtist_UpdatesArtist()
    {
        // Arrange
        var artist = await CreateTestArtistWithLibrary();
        artist.Name = "Updated Artist Name";
        artist.NameNormalized = "Updated Artist Name".ToNormalizedString()!;

        // Act
        var result = await GetArtistService().UpdateAsync(artist);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data);

        // Verify the update
        var updatedArtist = await GetArtistService().GetAsync(artist.Id);
        Assert.Equal("Updated Artist Name", updatedArtist.Data!.Name);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentArtist_ReturnsNotFound()
    {
        // Arrange
        var artist = new Artist
        {
            Id = 999999,
            Name = "Non Existent Artist",
            NameNormalized = "Non Existent Artist".ToNormalizedString()!,
            LibraryId = 1,
            Directory = "non-existent",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };

        // Act
        var result = await GetArtistService().UpdateAsync(artist);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
    }

    [Fact]
    public async Task UpdateAsync_NullArtist_ThrowsArgumentNullException()
    {
        // Arrange
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateAsync(null!));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ValidArtistIds_DeletesArtists()
    {
        // Arrange
        var artist1 = await CreateTestArtistWithLibrary();
        var artist2 = await CreateTestArtistWithLibrary("Test Artist 2");
        var artistIds = new[] { artist1.Id, artist2.Id };

        // Act
        var result = await GetArtistService().DeleteAsync(artistIds);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data);

        // Verify deletion
        var deletedArtist1 = await GetArtistService().GetAsync(artist1.Id);
        var deletedArtist2 = await GetArtistService().GetAsync(artist2.Id);
        Assert.Null(deletedArtist1.Data);
        Assert.Null(deletedArtist2.Data);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentArtistId_ReturnsError()
    {
        // Arrange
        var nonExistentIds = new[] { 999999 };

        // Act
        var result = await GetArtistService().DeleteAsync(nonExistentIds);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown artist", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    [Fact]
    public async Task DeleteAsync_EmptyArray_ThrowsArgumentException()
    {
        // Arrange
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteAsync(Array.Empty<int>()));
    }

    #endregion

    #region LockUnlockArtistAsync Tests

    [Fact]
    public async Task LockUnlockArtistAsync_LockArtist_LocksArtist()
    {
        // Arrange
        var artist = await CreateTestArtistWithLibrary();
        Assert.False(artist.IsLocked); // Ensure it starts unlocked

        // Act
        var result = await GetArtistService().LockUnlockArtistAsync(artist.Id, true);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data);

        // Verify the artist is locked
        var lockedArtist = await GetArtistService().GetAsync(artist.Id);
        Assert.True(lockedArtist.Data!.IsLocked);
    }

    [Fact]
    public async Task LockUnlockArtistAsync_UnlockArtist_UnlocksArtist()
    {
        // Arrange
        var artist = await CreateTestArtistWithLibrary();
        artist.IsLocked = true;
        await GetArtistService().UpdateAsync(artist);

        // Act
        var result = await GetArtistService().LockUnlockArtistAsync(artist.Id, false);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data);

        // Verify the artist is unlocked
        var unlockedArtist = await GetArtistService().GetAsync(artist.Id);
        Assert.False(unlockedArtist.Data!.IsLocked);
    }

    [Fact]
    public async Task LockUnlockArtistAsync_NonExistentArtist_ReturnsError()
    {
        // Act
        var result = await GetArtistService().LockUnlockArtistAsync(999999, true);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown artist", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    #endregion

    #region RescanAsync Tests

    [Fact]
    public async Task RescanAsync_ValidArtistIds_ReturnsSuccess()
    {
        // Arrange
        var artist = await CreateTestArtistWithLibrary();
        var artistIds = new[] { artist.Id };

        // Act
        var result = await GetArtistService().RescanAsync(artistIds);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task RescanAsync_NonExistentArtistId_ReturnsError()
    {
        // Arrange
        var nonExistentIds = new[] { 999999 };

        // Act
        var result = await GetArtistService().RescanAsync(nonExistentIds);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown artist", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    #endregion

    #region Helper Methods

    private async Task<Library> CreateTestLibrary()
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        var library = new Library
        {
            Name = "Test Library",
            Path = "/test/library/path",
            Type = (int)LibraryType.Storage,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.Libraries.Add(library);
        await context.SaveChangesAsync();
        return library;
    }

    private async Task<Artist> CreateTestArtistWithLibrary(string artistName = "Test Artist")
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        
        var library = new Library
        {
            Name = "Test Library",
            Path = "/test/library/path",
            Type = (int)LibraryType.Storage,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.Libraries.Add(library);
        await context.SaveChangesAsync();

        var artistModel = new Melodee.Common.Models.Artist(artistName, artistName.ToNormalizedString()!, null, null, library.Id);
        var artist = new Artist
        {
            ApiKey = Guid.NewGuid(),
            Directory = artistModel.ToDirectoryName(255),
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            LibraryId = library.Id,
            Name = artistName,
            NameNormalized = artistName.ToNormalizedString()!,
            Library = library
        };
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        return artist;
    }

    private async Task SeedTestArtists(int count = 5)
    {
        await using var context = await MockFactory().CreateDbContextAsync();
        
        var library = new Library
        {
            Name = "Test Library",
            Path = "/test/library/path",
            Type = (int)LibraryType.Storage,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
        };
        context.Libraries.Add(library);
        await context.SaveChangesAsync();

        for (int i = 1; i <= count; i++)
        {
            var artistName = $"Test Artist {i}";
            var artistModel = new Melodee.Common.Models.Artist(artistName, artistName.ToNormalizedString()!, null, null, library.Id);
            var artist = new Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = artistModel.ToDirectoryName(255),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!,
                AlbumCount = i,
                SongCount = i * 10,
                Library = library
            };
            context.Artists.Add(artist);
        }
        await context.SaveChangesAsync();
    }

    #endregion
}
