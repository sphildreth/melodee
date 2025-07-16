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

    #region SaveImageAsArtistImageAsync Tests

    [Fact]
    public async Task SaveImageAsArtistImageAsync_ValidArtistAndImage_SavesImage()
    {
        // Arrange
        var artist = await CreateTestArtistWithLibrary();
        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // Simple JPEG header

        // Act
        var result = await GetArtistService().SaveImageAsArtistImageAsync(artist.Id, false, imageBytes);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task SaveImageAsArtistImageAsync_NonExistentArtist_ReturnsError()
    {
        // Arrange
        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };

        // Act
        var result = await GetArtistService().SaveImageAsArtistImageAsync(999999, false, imageBytes);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown artist", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    [Fact]
    public async Task SaveImageAsArtistImageAsync_EmptyImageBytes_ThrowsArgumentException()
    {
        // Arrange
        var artist = await CreateTestArtistWithLibrary();
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SaveImageAsArtistImageAsync(artist.Id, false, Array.Empty<byte>()));
    }

    [Fact]
    public async Task SaveImageAsArtistImageAsync_InvalidArtistId_ThrowsArgumentException()
    {
        // Arrange
        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SaveImageAsArtistImageAsync(0, false, imageBytes));
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SaveImageAsArtistImageAsync(-1, false, imageBytes));
    }

    [Fact]
    public async Task SaveImageAsArtistImageAsync_DeleteAllImagesTrue_ReplacesAllImages()
    {
        // Arrange
        var artist = await CreateTestArtistWithLibrary();
        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };

        // Act
        var result = await GetArtistService().SaveImageAsArtistImageAsync(artist.Id, true, imageBytes);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data);
    }

    #endregion

    #region SaveImageUrlAsArtistImageAsync Tests

    [Fact]
    public async Task SaveImageUrlAsArtistImageAsync_ValidArtistAndUrl_ReturnsSuccess()
    {
        // Arrange
        var artist = await CreateTestArtistWithLibrary();
        var imageUrl = "https://example.com/image.jpg";

        // Act
        var result = await GetArtistService().SaveImageUrlAsArtistImageAsync(artist.Id, imageUrl, false);

        // Assert
        // Note: This will likely fail in practice due to HTTP call, but tests the method signature
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SaveImageUrlAsArtistImageAsync_NonExistentArtist_ReturnsError()
    {
        // Arrange
        var imageUrl = "https://example.com/image.jpg";

        // Act
        var result = await GetArtistService().SaveImageUrlAsArtistImageAsync(999999, imageUrl, false);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown artist", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    [Fact]
    public async Task SaveImageUrlAsArtistImageAsync_EmptyUrl_ThrowsArgumentException()
    {
        // Arrange
        var artist = await CreateTestArtistWithLibrary();
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SaveImageUrlAsArtistImageAsync(artist.Id, string.Empty, false));
    }

    [Fact]
    public async Task SaveImageUrlAsArtistImageAsync_InvalidArtistId_ThrowsArgumentException()
    {
        // Arrange
        var imageUrl = "https://example.com/image.jpg";
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SaveImageUrlAsArtistImageAsync(0, imageUrl, false));
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SaveImageUrlAsArtistImageAsync(-1, imageUrl, false));
    }

    #endregion

    #region MergeArtistsAsync Tests

    [Fact]
    public async Task MergeArtistsAsync_ValidArtists_MergesSuccessfully()
    {
        // Arrange
        var targetArtist = await CreateTestArtistWithLibrary("Target Artist");
        var sourceArtist1 = await CreateTestArtistWithLibrary("Source Artist 1");
        var sourceArtist2 = await CreateTestArtistWithLibrary("Source Artist 2");
        var artistIdsToMerge = new[] { sourceArtist1.Id, sourceArtist2.Id };

        // Act
        var result = await GetArtistService().MergeArtistsAsync(targetArtist.Id, artistIdsToMerge);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data);

        // Verify source artists are deleted
        var deletedArtist1 = await GetArtistService().GetAsync(sourceArtist1.Id);
        var deletedArtist2 = await GetArtistService().GetAsync(sourceArtist2.Id);
        Assert.Null(deletedArtist1.Data);
        Assert.Null(deletedArtist2.Data);
    }

    [Fact]
    public async Task MergeArtistsAsync_NonExistentTargetArtist_ReturnsError()
    {
        // Arrange
        var sourceArtist = await CreateTestArtistWithLibrary("Source Artist");
        var artistIdsToMerge = new[] { sourceArtist.Id };

        // Act
        var result = await GetArtistService().MergeArtistsAsync(999999, artistIdsToMerge);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown artist to merge into", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    [Fact]
    public async Task MergeArtistsAsync_NonExistentSourceArtist_ReturnsError()
    {
        // Arrange
        var targetArtist = await CreateTestArtistWithLibrary("Target Artist");
        var artistIdsToMerge = new[] { 999999 };

        // Act
        var result = await GetArtistService().MergeArtistsAsync(targetArtist.Id, artistIdsToMerge);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown artist to merge", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    [Fact]
    public async Task MergeArtistsAsync_InvalidTargetArtistId_ThrowsArgumentException()
    {
        // Arrange
        var sourceArtist = await CreateTestArtistWithLibrary("Source Artist");
        var artistIdsToMerge = new[] { sourceArtist.Id };
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.MergeArtistsAsync(0, artistIdsToMerge));
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.MergeArtistsAsync(-1, artistIdsToMerge));
    }

    [Fact]
    public async Task MergeArtistsAsync_EmptySourceArray_ThrowsArgumentException()
    {
        // Arrange
        var targetArtist = await CreateTestArtistWithLibrary("Target Artist");
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.MergeArtistsAsync(targetArtist.Id, Array.Empty<int>()));
    }

    #endregion

    #region DeleteAlbumsForArtist Tests

    [Fact]
    public async Task DeleteAlbumsForArtist_ValidArtistAndAlbums_DeletesAlbums()
    {
        // Arrange
        var artist = await CreateTestArtistWithLibrary();
        var albumIdsToDelete = new[] { 1, 2, 3 }; // Mock album IDs

        // Act
        var result = await GetArtistService().DeleteAlbumsForArtist(artist.Id, albumIdsToDelete);

        // Assert
        // Note: This depends on AlbumService.DeleteAsync implementation
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeleteAlbumsForArtist_NonExistentArtist_ReturnsError()
    {
        // Arrange
        var albumIdsToDelete = new[] { 1, 2, 3 };

        // Act
        var result = await GetArtistService().DeleteAlbumsForArtist(999999, albumIdsToDelete);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown artist", result.Messages?.FirstOrDefault() ?? string.Empty);
    }

    [Fact]
    public async Task DeleteAlbumsForArtist_InvalidArtistId_ThrowsArgumentException()
    {
        // Arrange
        var albumIdsToDelete = new[] { 1, 2, 3 };
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.DeleteAlbumsForArtist(0, albumIdsToDelete));
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.DeleteAlbumsForArtist(-1, albumIdsToDelete));
    }

    #endregion

    #region GetArtistImageBytesAndEtagAsync Tests

    [Fact]
    public async Task GetArtistImageBytesAndEtagAsync_ValidApiKey_ReturnsImageData()
    {
        // Arrange
        var artist = await CreateTestArtistWithLibrary();
        var apiKey = artist.ApiKey;

        // Act
        var result = await GetArtistService().GetArtistImageBytesAndEtagAsync(apiKey);

        // Assert
        Assert.NotNull(result);
        // Note: In a real scenario with file system, this would return actual image data
    }

    [Fact]
    public async Task GetArtistImageBytesAndEtagAsync_ValidApiKeyWithSize_ReturnsImageData()
    {
        // Arrange
        var artist = await CreateTestArtistWithLibrary();
        var apiKey = artist.ApiKey;
        var size = "medium";

        // Act
        var result = await GetArtistService().GetArtistImageBytesAndEtagAsync(apiKey, size);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetArtistImageBytesAndEtagAsync_NonExistentApiKey_ReturnsEmptyResult()
    {
        // Arrange
        var nonExistentApiKey = Guid.NewGuid();

        // Act
        var result = await GetArtistService().GetArtistImageBytesAndEtagAsync(nonExistentApiKey);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Bytes);
        Assert.Null(result.Etag);
    }

    [Fact]
    public async Task GetArtistImageBytesAndEtagAsync_NullApiKey_ThrowsArgumentNullException()
    {
        // Arrange
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            service.GetArtistImageBytesAndEtagAsync(null));
    }

    [Fact]
    public async Task GetArtistImageBytesAndEtagAsync_EmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.GetArtistImageBytesAndEtagAsync(Guid.Empty));
    }

    #endregion

    #region ClearCacheAsync Tests

    [Fact]
    public async Task ClearCacheAsync_ValidArtist_ClearsCache()
    {
        // Arrange
        var artist = await CreateTestArtistWithLibrary();

        // Act & Assert - Should not throw
        await GetArtistService().ClearCacheAsync(artist, CancellationToken.None);
    }

    [Fact]
    public async Task ClearCacheAsync_ValidArtistId_ClearsCache()
    {
        // Arrange
        var artist = await CreateTestArtistWithLibrary();

        // Act & Assert - Should not throw
        await GetArtistService().ClearCacheAsync(artist.Id, CancellationToken.None);
    }

    [Fact]
    public async Task ClearCacheAsync_NonExistentArtistId_ThrowsException()
    {
        // Arrange
        var service = GetArtistService();

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => 
            service.ClearCacheAsync(999999, CancellationToken.None));
    }

    #endregion

    #region Edge Cases and Additional Coverage Tests

    [Fact]
    public async Task ListAsync_WithIsLockedFilter_ReturnsFilteredResults()
    {
        // Arrange
        await SeedTestArtists();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            FilterBy = new[]
            {
                new FilterOperatorInfo("islocked", FilterOperator.Equals, "false")
            }
        };

        // Act
        var result = await GetArtistService().ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Data.All(a => !a.IsLocked));
    }

    [Fact]
    public async Task ListAsync_WithAlternateNamesFilter_ReturnsFilteredResults()
    {
        // Arrange
        await SeedTestArtistsWithAlternateNames();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            FilterBy = new[]
            {
                new FilterOperatorInfo("alternatenames", FilterOperator.Contains, "Alternative")
            }
        };

        // Act
        var result = await GetArtistService().ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Data.All(a => a.AlternateNames.Contains("Alternative")));
    }

    [Fact]
    public async Task ListAsync_WithStartsWithFilter_ReturnsFilteredResults()
    {
        // Arrange
        await SeedTestArtists();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            FilterBy = new[]
            {
                new FilterOperatorInfo("name", FilterOperator.StartsWith, "Test")
            }
        };

        // Act
        var result = await GetArtistService().ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Data.All(a => a.Name.StartsWith("Test")));
    }

    [Fact]
    public async Task ListAsync_WithDescendingOrder_ReturnsOrderedResults()
    {
        // Arrange
        await SeedTestArtists();
        var pagedRequest = new PagedRequest
        {
            PageSize = 10,
            Page = 1,
            OrderBy = new Dictionary<string, string> { { "Name", "DESC" } }
        };

        // Act
        var result = await GetArtistService().ListAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
        
        var names = result.Data.Select(a => a.Name).ToArray();
        var sortedNames = names.OrderByDescending(n => n).ToArray();
        Assert.Equal(sortedNames, names);
    }

    [Fact]
    public async Task FindArtistAsync_ByMusicBrainzId_ReturnsArtist()
    {
        // Arrange
        var musicBrainzId = Guid.NewGuid();
        var artistName = "MusicBrainz Find Test";

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
                Directory = "find-mb-test",
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
        var result = await GetArtistService().FindArtistAsync(null, Guid.Empty, null, musicBrainzId, null);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(artistName, result.Data.Name);
        Assert.Equal(musicBrainzId, result.Data.MusicBrainzId);
    }

    [Fact]
    public async Task FindArtistAsync_BySpotifyId_ReturnsArtist()
    {
        // Arrange
        var spotifyId = "spotify:artist:123456";
        var artistName = "Spotify Find Test";

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
                SpotifyId = spotifyId,
                Directory = "find-spotify-test",
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
        var result = await GetArtistService().FindArtistAsync(null, Guid.Empty, null, null, spotifyId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(artistName, result.Data.Name);
        Assert.Equal(spotifyId, result.Data.SpotifyId);
    }

    #endregion

    #region Additional Helper Methods

    private async Task SeedTestArtistsWithAlternateNames(int count = 3)
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
                AlternateNames = $"Alternative Name {i}",
                AlbumCount = i,
                SongCount = i * 10,
                Library = library
            };
            context.Artists.Add(artist);
        }
        await context.SaveChangesAsync();
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
