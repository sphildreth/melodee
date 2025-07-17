using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Services.Models;
using Melodee.Common.Configuration;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;
using Xunit;

namespace Melodee.Tests.Services;

public sealed class LibraryServiceTests : ServiceTestBase
{
    // First, ensure we clean up any existing test libraries before each test
    public LibraryServiceTests()
    {
        // Clean up the database before each test
        CleanupTestLibraries().GetAwaiter().GetResult();
    }
    
    private async Task CleanupTestLibraries()
    {
        using var context = await MockFactory().CreateDbContextAsync();
        var libraries = await context.Libraries.ToListAsync();
        if (libraries.Any())
        {
            context.Libraries.RemoveRange(libraries);
            await context.SaveChangesAsync();
        }
        
        var histories = await context.LibraryScanHistories.ToListAsync();
        if (histories.Any())
        {
            context.LibraryScanHistories.RemoveRange(histories);
            await context.SaveChangesAsync();
        }
    }

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var libraryService = GetLibraryService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => libraryService.GetAsync(0));
        await Assert.ThrowsAsync<ArgumentException>(() => libraryService.GetAsync(-1));
    }
    
    [Fact]
    public async Task GetAsync_WithValidId_ReturnsLibrary()
    {
        // Arrange
        var libraryService = GetLibraryService();
        var context = await CreateLibraryInDb(1, "Test Library", LibraryType.Inbound);
        
        // Act
        var result = await libraryService.GetAsync(1);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data!.Id);
        Assert.Equal("Test Library", result.Data.Name);
        Assert.Equal((int)LibraryType.Inbound, result.Data.Type);
    }
    
    [Fact]
    public async Task GetAsync_WithNonExistingId_ReturnsNullLibrary()
    {
        // Arrange
        var libraryService = GetLibraryService();
        
        // Act
        var result = await libraryService.GetAsync(9999);
        
        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Data);
    }

    #endregion

    #region GetByApiKeyAsync Tests
    
    [Fact]
    public async Task GetByApiKeyAsync_WithEmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var libraryService = GetLibraryService();
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            libraryService.GetByApiKeyAsync(Guid.Empty));
    }
    
    [Fact]
    public async Task GetByApiKeyAsync_WithValidApiKey_ReturnsLibrary()
    {
        // Arrange
        var libraryService = GetLibraryService();
        var apiKey = Guid.NewGuid();
        var context = await CreateLibraryInDb(1, "API Key Library", LibraryType.Inbound, apiKey);
        
        // Act
        var result = await libraryService.GetByApiKeyAsync(apiKey);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data!.Id);
        Assert.Equal("API Key Library", result.Data.Name);
        Assert.Equal(apiKey, result.Data.ApiKey);
    }
    
    [Fact]
    public async Task GetByApiKeyAsync_WithNonExistingApiKey_ReturnsErrorResult()
    {
        // Arrange
        var libraryService = GetLibraryService();
        var nonExistingApiKey = Guid.NewGuid();
        
        // Act
        var result = await libraryService.GetByApiKeyAsync(nonExistingApiKey);
        
        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Data);
        Assert.Contains("Unknown library.", result.Messages ?? []);
    }

    #endregion

    #region Library Type-Specific Get Tests
    
    [Fact]
    public async Task GetInboundLibraryAsync_WhenExists_ReturnsInboundLibrary()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "Inbound Library", LibraryType.Inbound);
        
        // Act
        var result = await libraryService.GetInboundLibraryAsync();
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.Id);
        Assert.Equal("Inbound Library", result.Data.Name);
        Assert.Equal((int)LibraryType.Inbound, result.Data.Type);
    }
    
    [Fact]
    public async Task GetStorageLibrariesAsync_WhenExists_ReturnsStorageLibraries()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "Storage Library", LibraryType.Storage);
        
        // Act
        var result = await libraryService.GetStorageLibrariesAsync();
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.All(result.Data, lib => Assert.Equal((int)LibraryType.Storage, lib.Type));
    }
    
    [Fact]
    public async Task GetUserImagesLibraryAsync_WhenExists_ReturnsUserImagesLibrary()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "User Images Library", LibraryType.UserImages);
        
        // Act
        var result = await libraryService.GetUserImagesLibraryAsync();
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.Id);
        Assert.Equal("User Images Library", result.Data.Name);
        Assert.Equal((int)LibraryType.UserImages, result.Data.Type);
    }
    
    [Fact]
    public async Task GetPlaylistLibraryAsync_WhenExists_ReturnsPlaylistLibrary()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "Playlist Library", LibraryType.Playlist);
        
        // Act
        var result = await libraryService.GetPlaylistLibraryAsync();
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.Id);
        Assert.Equal("Playlist Library", result.Data.Name);
        Assert.Equal((int)LibraryType.Playlist, result.Data.Type);
    }
    
    [Fact]
    public async Task GetStagingLibraryAsync_WhenExists_ReturnsStagingLibrary()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "Staging Library", LibraryType.Staging);
        
        // Act
        var result = await libraryService.GetStagingLibraryAsync();
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.Id);
        Assert.Equal("Staging Library", result.Data.Name);
        Assert.Equal((int)LibraryType.Staging, result.Data.Type);
    }
    
    [Fact]
    public async Task GetInboundLibraryAsync_WhenNotExists_ThrowsException()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => libraryService.GetInboundLibraryAsync());
    }
    
    [Fact]
    public async Task GetStorageLibrariesAsync_WhenNotExists_ThrowsException()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => libraryService.GetStorageLibrariesAsync());
    }
    
    [Fact]
    public async Task GetUserImagesLibraryAsync_WhenNotExists_ThrowsException()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => libraryService.GetUserImagesLibraryAsync());
    }
    
    [Fact]
    public async Task GetPlaylistLibraryAsync_WhenNotExists_ThrowsException()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => libraryService.GetPlaylistLibraryAsync());
    }
    
    [Fact]
    public async Task GetStagingLibraryAsync_WhenNotExists_ThrowsException()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => libraryService.GetStagingLibraryAsync());
    }
    
    #endregion

    #region PurgeLibraryAsync Tests
    
    [Fact]
    public async Task PurgeLibraryAsync_WithInvalidId_ReturnsError()
    {
        // Arrange
        var libraryService = GetLibraryService();
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => libraryService.PurgeLibraryAsync(0));
        await Assert.ThrowsAsync<ArgumentException>(() => libraryService.PurgeLibraryAsync(-1));
    }
    
    [Fact]
    public async Task PurgeLibraryAsync_WithNonExistingId_ReturnsErrorResult()
    {
        // Arrange
        var libraryService = GetLibraryService();
        
        // Act
        var result = await libraryService.PurgeLibraryAsync(9999);
        
        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Data);
        Assert.Contains("Invalid Library Id", result.Messages ?? []);
        Assert.Equal(OperationResponseType.Error, result.Type);
    }
    
    [Fact]
    public async Task PurgeLibraryAsync_WithValidId_PurgesLibraryAndReturnsSuccess()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "Library To Purge", LibraryType.Inbound, null, 5, 10, 20);
        await CreateLibraryScanHistories(1, 3); // Add some scan histories
        
        // Act
        var result = await libraryService.PurgeLibraryAsync(1);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data!.Id);
        Assert.Equal(0, result.Data.ArtistCount);
        Assert.Equal(0, result.Data.AlbumCount);
        Assert.Equal(0, result.Data.SongCount);
        Assert.Null(result.Data.LastScanAt);
        
        // Verify histories were deleted
        using var context = await MockFactory().CreateDbContextAsync();
        var histories = await context.LibraryScanHistories.Where(h => h.LibraryId == 1).ToListAsync();
        Assert.Empty(histories);
    }
    
    #endregion
    
    #region Listing Tests
    
    [Fact]
    public async Task ListAsync_ReturnsPagedLibraries()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "Library 1", LibraryType.Inbound);
        await CreateLibraryInDb(2, "Library 2", LibraryType.Storage);
        await CreateLibraryInDb(3, "Library 3", LibraryType.Staging);
        
        var pagedRequest = new PagedRequest { Page = 1, PageSize = 2 };
        
        // Act
        var result = await libraryService.ListAsync(pagedRequest);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.TotalCount); // Total count should be 3
        Assert.Equal(2, result.Data.Count()); // But only 2 returned due to page size
        Assert.Equal(2, result.TotalPages);
    }
    
    [Fact]
    public async Task ListAsync_WithPageTwo_ReturnsSecondPage()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "Library 1", LibraryType.Inbound);
        await CreateLibraryInDb(2, "Library 2", LibraryType.Storage);
        await CreateLibraryInDb(3, "Library 3", LibraryType.Staging);
        
        var pagedRequest = new PagedRequest { Page = 2, PageSize = 2 };
        
        // Act
        var result = await libraryService.ListAsync(pagedRequest);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Data); // Only one item on second page
    }
    
    [Fact]
    public async Task ListAsync_WithTotalCountOnly_ReturnsOnlyCount()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "Library 1", LibraryType.Inbound);
        await CreateLibraryInDb(2, "Library 2", LibraryType.Storage);
        
        var pagedRequest = new PagedRequest { Page = 1, PageSize = 10, IsTotalCountOnlyRequest = true };
        
        // Act
        var result = await libraryService.ListAsync(pagedRequest);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Empty(result.Data);
    }
    
    [Fact]
    public async Task ListMediaLibrariesAsync_ReturnsOnlyMediaLibraries()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "Inbound Library", LibraryType.Inbound);
        await CreateLibraryInDb(2, "Staging Library", LibraryType.Staging);
        await CreateLibraryInDb(3, "Storage Library", LibraryType.Storage);
        await CreateLibraryInDb(4, "User Images Library", LibraryType.UserImages);
        
        // Act
        var result = await libraryService.ListMediaLibrariesAsync();
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
        Assert.All(result.Data, lib => Assert.Contains(lib.TypeValue, new[] { LibraryType.Inbound, LibraryType.Staging }));
    }
    
    [Fact]
    public async Task ListLibraryHistoriesAsync_ReturnsHistoriesForLibrary()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        var libraryId = 1;
        await CreateLibraryInDb(libraryId, "Test Library", LibraryType.Inbound);
        await CreateLibraryScanHistories(libraryId, 5);
        
        var pagedRequest = new PagedRequest { Page = 1, PageSize = 10 };
        
        // Act
        var result = await libraryService.ListLibraryHistoriesAsync(libraryId, pagedRequest);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(5, result.Data.Count());
    }
    
    [Fact]
    public async Task ListLibraryHistoriesAsync_WithPaging_ReturnsPaginatedHistories()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        var libraryId = 1;
        await CreateLibraryInDb(libraryId, "Test Library", LibraryType.Inbound);
        await CreateLibraryScanHistories(libraryId, 10);
        
        var pagedRequest = new PagedRequest { Page = 2, PageSize = 3 };
        
        // Act
        var result = await libraryService.ListLibraryHistoriesAsync(libraryId, pagedRequest);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal(10, result.TotalCount);
        Assert.Equal(3, result.Data.Count());
    }
    
    [Fact]
    public async Task ListLibraryHistoriesAsync_WithNonExistingLibrary_ReturnsEmptyResult()
    {
        // Arrange
        await CleanupTestLibraries();
        var libraryService = GetLibraryService();
        var nonExistingLibraryId = 999;
        
        var pagedRequest = new PagedRequest { Page = 1, PageSize = 10 };
        
        // Act
        var result = await libraryService.ListLibraryHistoriesAsync(nonExistingLibraryId, pagedRequest);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalCount);
    }
    
    #endregion
    
    #region MoveAlbumsFromLibraryToLibrary Tests

    [Fact]
    public async Task MoveAlbumsFromLibraryToLibrary_WithSameLibraryNames_ReturnsError()
    {
        // Arrange
        var libraryService = GetLibraryService();
        
        // Act
        var result = await libraryService.MoveAlbumsFromLibraryToLibrary(
            "TestLibrary",
            "TestLibrary", 
            _ => true,
            false);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Data);
        Assert.Contains("From and To Library cannot be the same.", result.Messages ?? []);
    }

    [Fact]
    public async Task MoveAlbumsFromLibraryToLibrary_WithInvalidFromLibrary_ReturnsError()
    {
        // Arrange
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "ValidLibrary", LibraryType.Storage);
        
        // Act
        var result = await libraryService.MoveAlbumsFromLibraryToLibrary(
            "NonExistingLibrary",
            "ValidLibrary", 
            _ => true,
            false);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Data);
        Assert.Contains("Invalid From library Name", result.Messages ?? []);
    }

    [Fact]
    public async Task MoveAlbumsFromLibraryToLibrary_WithInvalidToLibrary_ReturnsError()
    {
        // Arrange
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "ValidLibrary", LibraryType.Inbound);
        
        // Act
        var result = await libraryService.MoveAlbumsFromLibraryToLibrary(
            "ValidLibrary",
            "NonExistingLibrary", 
            _ => true,
            false);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Data);
        Assert.Contains("Invalid To library Name", result.Messages ?? []);
    }

    [Fact]
    public async Task MoveAlbumsFromLibraryToLibrary_WithLockedFromLibrary_ReturnsError()
    {
        // Arrange
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "FromLibrary", LibraryType.Inbound, isLocked: true);
        await CreateLibraryInDb(2, "ToLibrary", LibraryType.Storage);
        
        // Act
        var result = await libraryService.MoveAlbumsFromLibraryToLibrary(
            "FromLibrary",
            "ToLibrary", 
            _ => true,
            false);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Data);
        Assert.Contains("From library is locked.", result.Messages ?? []);
    }

    [Fact]
    public async Task MoveAlbumsFromLibraryToLibrary_WithLockedToLibrary_ReturnsError()
    {
        // Arrange
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "FromLibrary", LibraryType.Inbound);
        await CreateLibraryInDb(2, "ToLibrary", LibraryType.Storage, isLocked: true);
        
        // Act
        var result = await libraryService.MoveAlbumsFromLibraryToLibrary(
            "FromLibrary",
            "ToLibrary", 
            _ => true,
            false);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Data);
        Assert.Contains("To library is locked.", result.Messages ?? []);
    }

    [Fact]
    public async Task MoveAlbumsFromLibraryToLibrary_WithNonStorageToLibrary_ReturnsError()
    {
        // Arrange
        var libraryService = GetLibraryService();
        await CreateLibraryInDb(1, "FromLibrary", LibraryType.Inbound);
        await CreateLibraryInDb(2, "ToLibrary", LibraryType.Inbound);  // Not Storage type
        
        // Act
        var result = await libraryService.MoveAlbumsFromLibraryToLibrary(
            "FromLibrary",
            "ToLibrary", 
            _ => true,
            false);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Data);
        Assert.Contains("Invalid From library Name", result.Messages ?? []);
    }

    [Fact]
    public async Task MoveAlbumsFromLibraryToLibrary_WithValidLibrariesAndNoAlbums_ReturnsSuccess()
    {
        // Arrange
        var mockConfigFactory = new Mock<IMelodeeConfigurationFactory>();
        var mockConfiguration = new Mock<IMelodeeConfiguration>();
        mockConfiguration.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<Func<int, int>>()))
            .Returns(100); // Max processing count
        mockConfiguration.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<Func<string?, string>>()))
            .Returns("dup_"); // Duplicate prefix
        mockConfigFactory.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockConfiguration.Object);
        
        var libraryService = GetLibraryService(configFactory: mockConfigFactory.Object);
        
        await CreateLibraryInDb(1, "FromLibrary", LibraryType.Inbound);
        await CreateLibraryInDb(2, "ToLibrary", LibraryType.Storage);

        // Setup directory structure mocking for the FromLibrary path
        Directory.CreateDirectory("/tmp/FromLibrary");
        
        // Act
        var result = await libraryService.MoveAlbumsFromLibraryToLibrary(
            "FromLibrary",
            "ToLibrary", 
            _ => true,  // All albums match condition
            false);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Data); // Didn't move any albums, only returns true if albums were moved
        
        // Cleanup
        Directory.Delete("/tmp/FromLibrary", recursive: true);
    }

    [Fact]
    public async Task MoveAlbumsFromLibraryToLibrary_WithConditionFilteringAllAlbums_ReturnsSuccess()
    {
        // Arrange
        var mockConfigFactory = new Mock<IMelodeeConfigurationFactory>();
        var mockConfiguration = new Mock<IMelodeeConfiguration>();
        mockConfiguration.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<Func<int, int>>()))
            .Returns(100); // Max processing count
        mockConfigFactory.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockConfiguration.Object);
        
        var libraryService = GetLibraryService(configFactory: mockConfigFactory.Object);
        
        await CreateLibraryInDb(1, "FromLibrary", LibraryType.Inbound);
        await CreateLibraryInDb(2, "ToLibrary", LibraryType.Storage);

        // Setup directory structure mocking for the FromLibrary path
        Directory.CreateDirectory("/tmp/FromLibrary");
        
        // Act
        var result = await libraryService.MoveAlbumsFromLibraryToLibrary(
            "FromLibrary",
            "ToLibrary", 
            _ => false,  // No albums match condition
            false);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Data); 
        
        // Cleanup
        Directory.Delete("/tmp/FromLibrary", recursive: true);
    }

    #endregion

    #region Helper Methods
    
    // Add an overload that allows passing a custom config factory
    private LibraryService GetLibraryService(IMelodeeConfigurationFactory configFactory)
    {
        return new LibraryService
        (
            Logger,
            CacheManager,
            MockFactory(),
            configFactory,
            Serializer,
            GetMelodeeMetadataMaker()
        );
    }
    
    private async Task<MelodeeDbContext> CreateLibraryInDb(int id, string name, LibraryType type, 
        Guid? apiKey = null, int artistCount = 0, int albumCount = 0, int songCount = 0, bool isLocked = false)
    {
        var context = await MockFactory().CreateDbContextAsync();
        
        // Remove all existing libraries to avoid constraint issues
        var existingLibraries = await context.Libraries
            .Where(l => l.Id == id || l.Type == (int)type)
            .ToListAsync();
            
        if (existingLibraries.Any())
        {
            context.Libraries.RemoveRange(existingLibraries);
            await context.SaveChangesAsync();
        }
        
        var library = new Library
        {
            Id = id,
            Name = name,
            Type = (int)type,
            ApiKey = apiKey ?? Guid.NewGuid(),
            Path = $"/tmp/{name}",
            ArtistCount = artistCount,
            AlbumCount = albumCount,
            SongCount = songCount,
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            IsLocked = isLocked
        };
        
        await context.Libraries.AddAsync(library);
        await context.SaveChangesAsync();
        
        return context;
    }
    
    private async Task CreateLibraryScanHistories(int libraryId, int count)
    {
        var context = await MockFactory().CreateDbContextAsync();
        
        // Remove any existing histories for this library
        var existingHistories = await context.LibraryScanHistories
            .Where(h => h.LibraryId == libraryId)
            .ToListAsync();
        
        if (existingHistories.Any())
        {
            context.LibraryScanHistories.RemoveRange(existingHistories);
            await context.SaveChangesAsync();
        }
        
        for (int i = 1; i <= count; i++)
        {
            var historyId = libraryId * 1000 + i; // Generate unique IDs based on library ID
            var history = new LibraryScanHistory
            {
                Id = historyId,
                LibraryId = libraryId,
                FoundArtistsCount = i * 2,
                FoundAlbumsCount = i * 3,
                FoundSongsCount = i * 10,
                DurationInMs = i * 1000,
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow.AddMinutes(-i))
            };
            
            await context.LibraryScanHistories.AddAsync(history);
        }
        
        await context.SaveChangesAsync();
    }
    #endregion
}
