using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using NodaTime;
using MelodeeModels = Melodee.Common.Models;
using DataModels = Melodee.Common.Data.Models;

namespace Melodee.Tests.Services;

public class AlbumServiceTests : ServiceTestBase
{
    [Fact]
    public async Task GetAsync_WithValidId_ReturnsAlbum()
    {
        // Arrange
        var albumName = "Test Album";
        var artistName = "Test Artist";
        var albumApiKey = Guid.NewGuid();
        var artistApiKey = Guid.NewGuid();
        var musicBrainzId = Guid.NewGuid();
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                ApiKey = Guid.NewGuid(),
                Name = "Test Library",
                Path = "/test/library",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new DataModels.Artist
            {
                ApiKey = artistApiKey,
                Directory = artistName.ToNormalizedString() ?? artistName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            var album = new DataModels.Album
            {
                ApiKey = albumApiKey,
                Directory = albumName.ToNormalizedString() ?? albumName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                ArtistId = artist.Id,
                Name = albumName,
                NameNormalized = albumName.ToNormalizedString()!,
                MusicBrainzId = musicBrainzId
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await GetAlbumService().GetAsync(1);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(albumName, result.Data.Name);
        Assert.Equal(albumApiKey, result.Data.ApiKey);
        Assert.Equal(musicBrainzId, result.Data.MusicBrainzId);
    }

    [Fact]
    public async Task GetAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var albumService = GetAlbumService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => albumService.GetAsync(0));
        await Assert.ThrowsAsync<ArgumentException>(() => albumService.GetAsync(-1));
    }

    [Fact]
    public async Task GetAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var albumService = GetAlbumService();

        // Act
        var result = await albumService.GetAsync(999);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetByApiKeyAsync_WithValidApiKey_ReturnsAlbum()
    {
        // Arrange
        var albumName = "Test Album";
        var artistName = "Test Artist";
        var albumApiKey = Guid.NewGuid();
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                ApiKey = Guid.NewGuid(),
                Name = "Test Library",
                Path = "/test/library",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new DataModels.Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = artistName.ToNormalizedString() ?? artistName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            var album = new DataModels.Album
            {
                ApiKey = albumApiKey,
                Directory = albumName.ToNormalizedString() ?? albumName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                ArtistId = artist.Id,
                Name = albumName,
                NameNormalized = albumName.ToNormalizedString()!
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await GetAlbumService().GetByApiKeyAsync(albumApiKey);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(albumName, result.Data.Name);
        Assert.Equal(albumApiKey, result.Data.ApiKey);
    }

    [Fact]
    public async Task GetByApiKeyAsync_WithEmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var albumService = GetAlbumService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => albumService.GetByApiKeyAsync(Guid.Empty));
    }

    [Fact]
    public async Task GetByApiKeyAsync_WithNonExistentApiKey_ReturnsNotFound()
    {
        // Arrange
        var albumService = GetAlbumService();
        var nonExistentApiKey = Guid.NewGuid();

        // Act
        var result = await albumService.GetByApiKeyAsync(nonExistentApiKey);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal("Unknown album.", result.Messages?.FirstOrDefault());
    }

    [Fact]
    public async Task GetByMusicBrainzIdAsync_WithValidId_ReturnsAlbum()
    {
        // Arrange
        var albumName = "Test Album";
        var artistName = "Test Artist";
        var musicBrainzId = Guid.NewGuid();
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                ApiKey = Guid.NewGuid(),
                Name = "Test Library",
                Path = "/test/library",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new DataModels.Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = artistName.ToNormalizedString() ?? artistName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            var album = new DataModels.Album
            {
                ApiKey = Guid.NewGuid(),
                Directory = albumName.ToNormalizedString() ?? albumName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                ArtistId = artist.Id,
                Name = albumName,
                NameNormalized = albumName.ToNormalizedString()!,
                MusicBrainzId = musicBrainzId
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();
        }

        // Act
        var result = await GetAlbumService().GetByMusicBrainzIdAsync(musicBrainzId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(albumName, result.Data.Name);
        Assert.Equal(musicBrainzId, result.Data.MusicBrainzId);
    }

    [Fact]
    public async Task GetByMusicBrainzIdAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var albumService = GetAlbumService();
        var nonExistentMusicBrainzId = Guid.NewGuid();

        // Act
        var result = await albumService.GetByMusicBrainzIdAsync(nonExistentMusicBrainzId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal("Unknown album.", result.Messages?.FirstOrDefault());
    }

    [Fact]
    public async Task ListAsync_WithValidRequest_ReturnsPagedResult()
    {
        // Arrange
        var albumNames = new[] { "Album A", "Album B", "Album C" };
        var artistName = "Test Artist";
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                ApiKey = Guid.NewGuid(),
                Name = "Test Library",
                Path = "/test/library",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new DataModels.Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = artistName.ToNormalizedString() ?? artistName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            foreach (var albumName in albumNames)
            {
                var album = new DataModels.Album
                {
                    ApiKey = Guid.NewGuid(),
                    Directory = albumName.ToNormalizedString() ?? albumName,
                    CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                    ArtistId = artist.Id,
                    Name = albumName,
                    NameNormalized = albumName.ToNormalizedString()!
                };
                context.Albums.Add(album);
            }
            await context.SaveChangesAsync();
        }

        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await GetAlbumService().ListAsync(pagedRequest);

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(3, result.Data.Count());
        Assert.Equal("Album A", result.Data.First().Name);
    }

    [Fact]
    public async Task ListForArtistApiKeyAsync_WithValidArtistApiKey_ReturnsAlbumsForArtist()
    {
        // Arrange
        var artistApiKey = Guid.NewGuid();
        var otherArtistApiKey = Guid.NewGuid();
        var albumNames = new[] { "Album 1", "Album 2" };
        var otherAlbumName = "Other Album";
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                ApiKey = Guid.NewGuid(),
                Name = "Test Library",
                Path = "/test/library",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new DataModels.Artist
            {
                ApiKey = artistApiKey,
                Directory = "Test Artist".ToNormalizedString() ?? "Test Artist",
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = "Test Artist",
                NameNormalized = "Test Artist".ToNormalizedString()!
            };
            context.Artists.Add(artist);

            var otherArtist = new DataModels.Artist
            {
                ApiKey = otherArtistApiKey,
                Directory = "Other Artist".ToNormalizedString() ?? "Other Artist",
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = "Other Artist",
                NameNormalized = "Other Artist".ToNormalizedString()!
            };
            context.Artists.Add(otherArtist);
            await context.SaveChangesAsync();

            foreach (var albumName in albumNames)
            {
                var album = new DataModels.Album
                {
                    ApiKey = Guid.NewGuid(),
                    Directory = albumName.ToNormalizedString() ?? albumName,
                    CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                    ArtistId = artist.Id,
                    Name = albumName,
                    NameNormalized = albumName.ToNormalizedString()!
                };
                context.Albums.Add(album);
            }

            var otherAlbum = new DataModels.Album
            {
                ApiKey = Guid.NewGuid(),
                Directory = otherAlbumName.ToNormalizedString() ?? otherAlbumName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                ArtistId = otherArtist.Id,
                Name = otherAlbumName,
                NameNormalized = otherAlbumName.ToNormalizedString()!
            };
            context.Albums.Add(otherAlbum);
            await context.SaveChangesAsync();
        }

        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await GetAlbumService().ListForArtistApiKeyAsync(pagedRequest, artistApiKey);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Data.Count());
        Assert.All(result.Data, album => Assert.Equal(artistApiKey, album.ArtistApiKey));
    }

    [Fact]
    public async Task AddAlbumAsync_WithValidAlbum_AddsAlbumSuccessfully()
    {
        // Arrange
        var albumName = "New Album";
        var artistName = "Test Artist";

        DataModels.Artist? artist;
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {

            var libraryStorageType = (int)LibraryType.Storage;
            var library = context.Libraries.First(x => x.Type == libraryStorageType);
            artist = new DataModels.Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = artistName.ToNormalizedString() ?? artistName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

        }

        var album = new DataModels.Album
        {
            ArtistId = 1,
            Artist = artist,
            Name = albumName,
            NameNormalized = albumName.ToNormalizedString()!,
            Directory = albumName.ToNormalizedString() ?? albumName,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            AlbumStatus = (short)AlbumStatus.Ok,
            ReleaseDate = LocalDate.FromDateOnly(DateOnly.FromDateTime(DateTime.Now))
        };
        
        
        // Act
        var result = await GetAlbumService().AddAlbumAsync(album);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(albumName, result.Data.Name);
        Assert.NotEqual(Guid.Empty, result.Data.ApiKey);
        Assert.NotNull(result.Data.Directory);
        
    }

    [Fact]
    public async Task AddAlbumAsync_WithNullAlbum_ThrowsArgumentException()
    {
        // Arrange
        var albumService = GetAlbumService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => albumService.AddAlbumAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithValidAlbum_UpdatesAlbumSuccessfully()
    {
        // Arrange
        var albumName = "Original Album";
        var updatedAlbumName = "Updated Album";
        var artistName = "Test Artist";
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                ApiKey = Guid.NewGuid(),
                Name = "Test Library",
                Path = "/test/library",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new DataModels.Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = artistName.ToNormalizedString() ?? artistName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            var album = new DataModels.Album
            {
                ApiKey = Guid.NewGuid(),
                Directory = albumName.ToNormalizedString() ?? albumName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                ArtistId = artist.Id,
                Name = albumName,
                NameNormalized = albumName.ToNormalizedString()!,
                AlbumStatus = (short)AlbumStatus.Ok
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();
        }

        // Get the album and update it
        var getResult = await GetAlbumService().GetAsync(1);
        var albumToUpdate = getResult.Data!;
        albumToUpdate.Name = updatedAlbumName;
        albumToUpdate.NameNormalized = updatedAlbumName.ToNormalizedString()!;

        // Act
        var result = await GetAlbumService().UpdateAsync(albumToUpdate);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data);

        // Verify the update
        var updatedAlbum = await GetAlbumService().GetAsync(1);
        Assert.Equal(updatedAlbumName, updatedAlbum.Data!.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithNullAlbum_ThrowsArgumentException()
    {
        // Arrange
        var albumService = GetAlbumService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => albumService.UpdateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentAlbum_ReturnsNotFound()
    {
        // Arrange
        var album = new DataModels.Album
        {
            Id = 999,
            ArtistId = 1,
            Name = "Non-existent Album",
            NameNormalized = "Non-existent Album".ToNormalizedString()!,
            Directory = "Non-existent Album".ToNormalizedString() ?? "Non-existent Album",
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            AlbumStatus = (short)AlbumStatus.Ok
        };

        // Act
        var result = await GetAlbumService().UpdateAsync(album);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.False(result.Data);
        Assert.Equal(OperationResponseType.NotFound, result.Type);
    }

    [Fact]
    public async Task DeleteAsync_WithValidAlbumIds_DeletesAlbumsSuccessfully()
    {
        // Arrange
        var albumNames = new[] { "Album 1", "Album 2" };
        var artistName = "Test Artist";
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                ApiKey = Guid.NewGuid(),
                Name = "Test Library",
                Path = "/test/library",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new DataModels.Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = artistName.ToNormalizedString() ?? artistName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            foreach (var albumName in albumNames)
            {
                var album = new DataModels.Album
                {
                    ApiKey = Guid.NewGuid(),
                    Directory = albumName.ToNormalizedString() ?? albumName,
                    CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                    ArtistId = artist.Id,
                    Name = albumName,
                    NameNormalized = albumName.ToNormalizedString()!,
                    AlbumStatus = (short)AlbumStatus.Ok
                };
                context.Albums.Add(album);
            }
            await context.SaveChangesAsync();
        }

        var albumIds = new[] { 1, 2 };

        // Act
        var result = await GetAlbumService().DeleteAsync(albumIds);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data);

        // Verify albums were deleted
        var album1Result = await GetAlbumService().GetAsync(1);
        var album2Result = await GetAlbumService().GetAsync(2);
        Assert.Null(album1Result.Data);
        Assert.Null(album2Result.Data);
    }

    [Fact]
    public async Task DeleteAsync_WithEmptyAlbumIds_ThrowsArgumentException()
    {
        // Arrange
        var albumService = GetAlbumService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => albumService.DeleteAsync(Array.Empty<int>()));
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentAlbumId_ReturnsFailure()
    {
        // Arrange
        var albumService = GetAlbumService();
        var nonExistentIds = new[] { 999 };

        // Act
        var result = await albumService.DeleteAsync(nonExistentIds);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.False(result.Data);
        Assert.Equal("Unknown album.", result.Messages?.FirstOrDefault());
    }

    [Fact]
    public async Task FindAlbumAsync_WithExistingAlbum_ReturnsAlbum()
    {
        // Arrange
        var albumName = "Test Album";
        var artistName = "Test Artist";
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                ApiKey = Guid.NewGuid(),
                Name = "Test Library",
                Path = "/test/library",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new DataModels.Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = artistName.ToNormalizedString() ?? artistName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            var album = new DataModels.Album
            {
                ApiKey = Guid.NewGuid(),
                Directory = albumName.ToNormalizedString() ?? albumName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                ArtistId = artist.Id,
                Name = albumName,
                NameNormalized = albumName.ToNormalizedString()!,
                AlbumStatus = (short)AlbumStatus.Ok
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();
        }

        var melodeeAlbum = new MelodeeModels.Album
        {
            ViaPlugins = [],
            OriginalDirectory = new FileSystemDirectoryInfo { Path = "/test", Name = "test" },
            Directory = new FileSystemDirectoryInfo { Path = "/test", Name = "test" },
            Tags = [new MetaTag<object?> { Identifier = MetaTagIdentifier.Album, Value = albumName }]
        };

        // Act
        var result = await GetAlbumService().FindAlbumAsync(1, melodeeAlbum);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.NotNull(result.Data);
        Assert.Equal(albumName, result.Data.Name);
    }

    [Fact]
    public async Task FindAlbumAsync_WithNonExistentAlbum_ReturnsNotFound()
    {
        // Arrange
        var melodeeAlbum = new MelodeeModels.Album
        {
            ViaPlugins = [],
            OriginalDirectory = new FileSystemDirectoryInfo { Path = "/test", Name = "test" },
            Directory = new FileSystemDirectoryInfo { Path = "/test", Name = "test" },
            Tags = [new MetaTag<object?> { Identifier = MetaTagIdentifier.Album, Value = "Non-existent Album" }]
        };

        // Act
        var result = await GetAlbumService().FindAlbumAsync(1, melodeeAlbum);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal("Unknown album.", result.Messages?.FirstOrDefault());
    }

    [Fact]
    public async Task LockUnlockAlbumAsync_WithValidAlbumId_TogglesLockStatus()
    {
        // Arrange
        var albumName = "Test Album";
        var artistName = "Test Artist";
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                ApiKey = Guid.NewGuid(),
                Name = "Test Library",
                Path = "/test/library",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new DataModels.Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = artistName.ToNormalizedString() ?? artistName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            var album = new DataModels.Album
            {
                ApiKey = Guid.NewGuid(),
                Directory = albumName.ToNormalizedString() ?? albumName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                ArtistId = artist.Id,
                Name = albumName,
                NameNormalized = albumName.ToNormalizedString()!,
                AlbumStatus = (short)AlbumStatus.Ok,
                IsLocked = false
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();
        }

        // Act - Lock the album
        var lockResult = await GetAlbumService().LockUnlockAlbumAsync(1, true);

        // Assert
        AssertResultIsSuccessful(lockResult);
        Assert.True(lockResult.Data);

        // Verify the album is locked
        var lockedAlbum = await GetAlbumService().GetAsync(1);
        Assert.True(lockedAlbum.Data!.IsLocked);

        // Act - Unlock the album
        var unlockResult = await GetAlbumService().LockUnlockAlbumAsync(1, false);

        // Assert
        AssertResultIsSuccessful(unlockResult);
        Assert.True(unlockResult.Data);

        // Verify the album is unlocked
        var unlockedAlbum = await GetAlbumService().GetAsync(1);
        Assert.False(unlockedAlbum.Data!.IsLocked);
    }

    [Fact]
    public async Task LockUnlockAlbumAsync_WithInvalidAlbumId_ThrowsArgumentException()
    {
        // Arrange
        var albumService = GetAlbumService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => albumService.LockUnlockAlbumAsync(0, true));
        await Assert.ThrowsAsync<ArgumentException>(() => albumService.LockUnlockAlbumAsync(-1, false));
    }

    [Fact]
    public async Task LockUnlockAlbumAsync_WithNonExistentAlbumId_ReturnsFailure()
    {
        // Act
        var result = await GetAlbumService().LockUnlockAlbumAsync(999, true);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.False(result.Data);
        Assert.Contains("Unknown album", result.Messages?.FirstOrDefault() ?? "");
    }

    [Fact]
    public async Task RescanAsync_WithValidAlbumIds_TriggersRescanEvents()
    {
        // Arrange
        var albumName = "Test Album";
        var artistName = "Test Artist";
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                ApiKey = Guid.NewGuid(),
                Name = "Test Library",
                Path = "/test/library",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new DataModels.Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = artistName.ToNormalizedString() ?? artistName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            var album = new DataModels.Album
            {
                ApiKey = Guid.NewGuid(),
                Directory = albumName.ToNormalizedString() ?? albumName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                ArtistId = artist.Id,
                Name = albumName,
                NameNormalized = albumName.ToNormalizedString()!,
                AlbumStatus = (short)AlbumStatus.Ok
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();
        }

        var albumIds = new[] { 1 };

        // Act
        var result = await GetAlbumService().RescanAsync(albumIds);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.False(result.Data); // This will be false since the directory doesn't actually exist
    }

    [Fact]
    public async Task RescanAsync_WithEmptyAlbumIds_ThrowsArgumentException()
    {
        // Arrange
        var albumService = GetAlbumService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => albumService.RescanAsync(Array.Empty<int>()));
    }

    [Fact]
    public async Task ClearCacheAsync_WithValidAlbumId_ClearsCache()
    {
        // Arrange
        var albumName = "Test Album";
        var artistName = "Test Artist";
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                ApiKey = Guid.NewGuid(),
                Name = "Test Library",
                Path = "/test/library",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new DataModels.Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = artistName.ToNormalizedString() ?? artistName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            var album = new DataModels.Album
            {
                ApiKey = Guid.NewGuid(),
                Directory = albumName.ToNormalizedString() ?? albumName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                ArtistId = artist.Id,
                Name = albumName,
                NameNormalized = albumName.ToNormalizedString()!,
                AlbumStatus = (short)AlbumStatus.Ok
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();
        }

        // Act & Assert - Should not throw any exceptions
        await GetAlbumService().ClearCacheAsync(1);
    }

    [Fact]
    public async Task ClearCacheForArtist_WithValidArtistId_ClearsCacheForAllArtistAlbums()
    {
        // Arrange
        var albumNames = new[] { "Album 1", "Album 2" };
        var artistName = "Test Artist";
        
        await using (var context = await MockFactory().CreateDbContextAsync())
        {
            var library = new Library
            {
                ApiKey = Guid.NewGuid(),
                Name = "Test Library",
                Path = "/test/library",
                Type = (int)LibraryType.Storage,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            context.Libraries.Add(library);
            await context.SaveChangesAsync();

            var artist = new DataModels.Artist
            {
                ApiKey = Guid.NewGuid(),
                Directory = artistName.ToNormalizedString() ?? artistName,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                LibraryId = library.Id,
                Name = artistName,
                NameNormalized = artistName.ToNormalizedString()!
            };
            context.Artists.Add(artist);
            await context.SaveChangesAsync();

            foreach (var albumName in albumNames)
            {
                var album = new DataModels.Album
                {
                    ApiKey = Guid.NewGuid(),
                    Directory = albumName.ToNormalizedString() ?? albumName,
                    CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                    ArtistId = artist.Id,
                    Name = albumName,
                    NameNormalized = albumName.ToNormalizedString()!,
                    AlbumStatus = (short)AlbumStatus.Ok
                };
                context.Albums.Add(album);
            }
            await context.SaveChangesAsync();
        }

        // Act & Assert - Should not throw any exceptions
        await GetAlbumService().ClearCacheForArtist(1);
    }

    protected new void AssertResultIsSuccessful<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, $"Operation failed: {string.Join(", ", result.Messages ?? Array.Empty<string>())}");
    }
}
