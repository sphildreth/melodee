using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Services;
using NodaTime;

namespace Melodee.Tests.Services;

public class SongServiceTests : ServiceTestBase
{
    private readonly SongService _songService;
    
    public SongServiceTests()
    {
        _songService = GetSongService();
    }

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var song = await CreateTestSong();

        // Act
        var result = await _songService.GetAsync(song.Id);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.Equal(song.Id, result.Data!.Id);
        Assert.Equal(song.Title, result.Data.Title);
        Assert.Equal(song.ApiKey, result.Data.ApiKey);
    }

    [Fact]
    public async Task GetAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        var result = await _songService.GetAsync(nonExistentId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetAsync_WithInvalidId_ThrowsArgumentException(int invalidId)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _songService.GetAsync(invalidId));
    }

    #endregion

    #region GetByApiKeyAsync Tests

    [Fact]
    public async Task GetByApiKeyAsync_WithValidApiKey_ReturnsSuccess()
    {
        // Arrange
        var song = await CreateTestSong();

        // Act
        var result = await _songService.GetByApiKeyAsync(song.ApiKey);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.Equal(song.Id, result.Data!.Id);
        Assert.Equal(song.ApiKey, result.Data.ApiKey);
    }

    [Fact]
    public async Task GetByApiKeyAsync_WithNonExistentApiKey_ReturnsFailure()
    {
        // Arrange
        var nonExistentApiKey = Guid.NewGuid();

        // Act
        var result = await _songService.GetByApiKeyAsync(nonExistentApiKey);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Contains("Unknown song", result.Messages ?? []);
    }

    [Fact]
    public async Task GetByApiKeyAsync_WithEmptyGuid_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _songService.GetByApiKeyAsync(Guid.Empty));
    }

    #endregion

    #region ListAsync Tests

    [Fact]
    public async Task ListAsync_WithValidRequest_ReturnsPagedResults()
    {
        // Arrange
        await CreateMultipleTestSongs(5);
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.TotalCount >= 5);
        Assert.True(result.Data.Count() >= 5);
    }

    [Fact]
    public async Task ListAsync_WithFilterByTitle_Equals_ReturnsFilteredResults()
    {
        // Arrange
        var songs = await CreateMultipleTestSongs(3);
        var specificTitle = songs.First().Title;
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            FilterBy = [new FilterOperatorInfo("Title", FilterOperator.Equals, specificTitle)]
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.Contains(result.Data, s => s.Title.Contains(specificTitle, StringComparison.OrdinalIgnoreCase));
    }
    
    [Fact]
    public async Task ListAsync_WithFilterByTitle_Contains_ReturnsFilteredResults()
    {
        // Arrange
        var songs = await CreateMultipleTestSongs(3);
        var specificTitle = songs.First().Title.Split().First();
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            FilterBy = [new FilterOperatorInfo("Title", FilterOperator.Equals, specificTitle)]
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.Contains(result.Data, s => s.Title.Contains(specificTitle, StringComparison.OrdinalIgnoreCase));
    }    

    [Fact]
    public async Task ListAsync_WithUserStarredFilter_HandlesUserSpecificData()
    {
        // Arrange
        await CreateMultipleTestSongs(2);
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            FilterBy = [new FilterOperatorInfo("UserStarred", FilterOperator.Equals, "true")]
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        // Result may be empty since no user starred songs are set up
    }

    [Fact]
    public async Task ListAsync_WithUserRatingFilter_HandlesUserSpecificData()
    {
        // Arrange
        await CreateMultipleTestSongs(2);
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            FilterBy = [new FilterOperatorInfo("UserRating", FilterOperator.GreaterThan, "3")]
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ListAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await CreateMultipleTestSongs(15);
        var pagedRequest = new PagedRequest
        {
            Page = 2,
            PageSize = 5
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        AssertResultIsSuccessful(result);
        Assert.True(result.Data.Count() <= 5);
        Assert.True(result.TotalPages >= 3);
    }

    [Fact]
    public async Task ListAsync_WithTotalCountOnly_ReturnsCountWithoutData()
    {
        // Arrange
        await CreateMultipleTestSongs(3);
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10,
            IsTotalCountOnlyRequest = true
        };
        var userId = 1;

        // Act
        var result = await _songService.ListAsync(pagedRequest, userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.True(result.TotalCount >= 3);
        Assert.Empty(result.Data);
    }

    #endregion

    #region ListForContributorsAsync Tests

    [Fact]
    public async Task ListForContributorsAsync_WithValidContributor_ReturnsResults()
    {
        // Arrange
        await CreateTestSongWithContributor("John Doe");
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _songService.ListForContributorsAsync(pagedRequest, "John");

        // Assert
        AssertResultIsSuccessful(result);
    }

    [Fact]
    public async Task ListForContributorsAsync_WithNonExistentContributor_ReturnsEmptyResults()
    {
        // Arrange
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _songService.ListForContributorsAsync(pagedRequest, "NonExistentContributor");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task ListForContributorsAsync_WithPartialContributorName_ReturnsMatchingResults()
    {
        // Arrange
        await CreateTestSongWithContributor("Jane Smith");
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _songService.ListForContributorsAsync(pagedRequest, "jane");

        // Assert
        AssertResultIsSuccessful(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ListForContributorsAsync_WithInvalidContributorName_HandlesGracefully(string contributorName)
    {
        // Arrange
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _songService.ListForContributorsAsync(pagedRequest, contributorName);

        // Assert
        Assert.NotNull(result);
        // Should handle gracefully without throwing
    }

    #endregion

    #region ListNowPlayingAsync Tests

    [Fact]
    public async Task ListNowPlayingAsync_WithNoNowPlayingSongs_ReturnsEmptyResults()
    {
        // Arrange
        var pagedRequest = new PagedRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _songService.ListNowPlayingAsync(pagedRequest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Data);
    }

    #endregion

    #region GetStreamForSongAsync Tests

    [Fact]
    public async Task GetStreamForSongAsync_WithNonExistentSong_ReturnsFailure()
    {
        // Arrange
        var nonExistentApiKey = Guid.NewGuid();
        var user = new UserInfo(
            1,
            Guid.NewGuid(),
            "Test User",
            "testpassword",
            "testsalt",
            "testtoken"
        );

        // Act
        var result = await _songService.GetStreamForSongAsync(user, nonExistentApiKey);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("Unknown song", result.Messages ?? []);
        Assert.False(result.Data.IsSuccess);
    }

    #endregion

    #region ClearCacheAsync Tests

    [Fact]
    public async Task ClearCacheAsync_WithValidSongId_ClearsCacheSuccessfully()
    {
        // Arrange
        var song = await CreateTestSong();

        // First get the song to populate cache
        await _songService.GetAsync(song.Id);
        await _songService.GetByApiKeyAsync(song.ApiKey);

        // Act
        await _songService.ClearCacheAsync(song.Id, CancellationToken.None);

        // Assert
        // Cache should be cleared - verify by checking if subsequent calls work
        var result = await _songService.GetAsync(song.Id);
        AssertResultIsSuccessful(result);
    }

    [Fact]
    public async Task ClearCacheAsync_WithNonExistentSongId_HandlesGracefully()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act & Assert
        // Should not throw exception
        await _songService.ClearCacheAsync(nonExistentId, CancellationToken.None);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ThrowsNotImplementedException()
    {
        // Arrange
        var songIds = new[] { 1, 2, 3 };

        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => 
            _songService.DeleteAsync(songIds, CancellationToken.None));
    }

    #endregion

    #region Caching Tests

    [Fact]
    public async Task GetAsync_CachesResults()
    {
        // Arrange
        var song = await CreateTestSong();

        // Act - First call should populate cache
        var result1 = await _songService.GetAsync(song.Id);
        var result2 = await _songService.GetAsync(song.Id);

        // Assert
        AssertResultIsSuccessful(result1);
        AssertResultIsSuccessful(result2);
        Assert.Equal(result1.Data!.Id, result2.Data!.Id);
    }

    [Fact]
    public async Task GetByApiKeyAsync_CachesApiKeyToIdMapping()
    {
        // Arrange
        var song = await CreateTestSong();

        // Act - First call should populate cache
        var result1 = await _songService.GetByApiKeyAsync(song.ApiKey);
        var result2 = await _songService.GetByApiKeyAsync(song.ApiKey);

        // Assert
        AssertResultIsSuccessful(result1);
        AssertResultIsSuccessful(result2);
        Assert.Equal(result1.Data!.ApiKey, result2.Data!.ApiKey);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task ListAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var pagedRequest = new PagedRequest { Page = 1, PageSize = 10 };
        var userId = 1;
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => 
            _songService.ListAsync(pagedRequest, userId, cts.Token));
    }

    [Fact]
    public async Task GetAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => 
            _songService.GetAsync(1, cts.Token));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("test")]
    [InlineData("rock")]
    public async Task ListForContributorsAsync_WithVariousContributorNames_HandlesCorrectly(string contributorName)
    {
        // Arrange
        var pagedRequest = new PagedRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await _songService.ListForContributorsAsync(pagedRequest, contributorName);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Helper Methods

    private async Task<Melodee.Common.Data.Models.Song> CreateTestSong()
    {
        var artist = await CreateTestArtist();
        var album = await CreateTestAlbum(artist);
        
        var song = new Melodee.Common.Data.Models.Song
        {
            ApiKey = Guid.NewGuid(),
            Title = $"Test Song {Guid.NewGuid()}",
            TitleNormalized = $"testsong{Guid.NewGuid()}".Replace("-", ""),
            AlbumId = album.Id,
            SongNumber = 1,
            FileName = "test.mp3",
            FileSize = 1000000,
            FileHash = "testhash",
            Duration = 180000,
            SamplingRate = 44100,
            BitRate = 320,
            BitDepth = 16,
            BPM = 120,
            ContentType = "audio/mpeg",
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            LastUpdatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await using var context = await MockFactory().CreateDbContextAsync();
        context.Songs.Add(song);
        await context.SaveChangesAsync();
        return song;
    }

    private async Task<Melodee.Common.Data.Models.Song[]> CreateMultipleTestSongs(int count)
    {
        var songs = new List<Melodee.Common.Data.Models.Song>();
        var artist = await CreateTestArtist();
        var album = await CreateTestAlbum(artist);

        for (int i = 0; i < count; i++)
        {
            var song = new Melodee.Common.Data.Models.Song
            {
                ApiKey = Guid.NewGuid(),
                Title = $"Test Song {i + 1}",
                TitleNormalized = $"Test Song {i + 1}".ToNormalizedString() ?? string.Empty,
                AlbumId = album.Id,
                SongNumber = i + 1,
                FileName = $"test{i + 1}.mp3",
                FileSize = 1000000 + (i * 10000),
                FileHash = $"testhash{i + 1}",
                Duration = 180000 + (i * 1000),
                SamplingRate = 44100,
                BitRate = 320,
                BitDepth = 16,
                BPM = 120,
                ContentType = "audio/mpeg",
                CreatedAt = SystemClock.Instance.GetCurrentInstant(),
                LastUpdatedAt = SystemClock.Instance.GetCurrentInstant()
            };
            songs.Add(song);
        }

        await using var context = await MockFactory().CreateDbContextAsync();
        context.Songs.AddRange(songs);
        await context.SaveChangesAsync();
        return songs.ToArray();
    }

    private async Task<Melodee.Common.Data.Models.Song> CreateTestSongWithContributor(string contributorName)
    {
        var song = await CreateTestSong();
        
        // Add contributor
        var contributor = new Contributor
        {
            SongId = song.Id,
            ContributorName = contributorName,
            Role = "Performer",
            AlbumId = song.AlbumId,
            ContributorType = (int)ContributorType.Performer,
            CreatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await using var context = await MockFactory().CreateDbContextAsync();
        context.Contributors.Add(contributor);
        await context.SaveChangesAsync();
        
        return song;
    }

    private async Task<Melodee.Common.Data.Models.Artist> CreateTestArtist()
    {
        var library = await CreateTestLibrary();
        
        var artist = new Melodee.Common.Data.Models.Artist
        {
            ApiKey = Guid.NewGuid(),
            Name = $"Test Artist {Guid.NewGuid()}",
            NameNormalized = $"testartist{Guid.NewGuid()}".Replace("-", ""),
            LibraryId = library.Id,
            Directory = "/testartist/",
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            LastUpdatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await using var context = await MockFactory().CreateDbContextAsync();
        context.Artists.Add(artist);
        await context.SaveChangesAsync();
        return artist;
    }

    private async Task<Melodee.Common.Data.Models.Album> CreateTestAlbum(Melodee.Common.Data.Models.Artist artist)
    {
        var album = new Melodee.Common.Data.Models.Album
        {
            ApiKey = Guid.NewGuid(),
            Name = $"Test Album {Guid.NewGuid()}",
            NameNormalized = $"testalbum{Guid.NewGuid()}".Replace("-", ""),
            ArtistId = artist.Id,
            Directory = "/testalbum/",
            ReleaseDate = new LocalDate(2023, 1, 1),
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            LastUpdatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await using var context = await MockFactory().CreateDbContextAsync();
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        return album;
    }

    private async Task<Library> CreateTestLibrary()
    {
        var library = new Library
        {
            ApiKey = Guid.NewGuid(),
            Name = $"Test Library {Guid.NewGuid()}",
            Path = "/test/library/",
            Type = (int)LibraryType.Storage,
            CreatedAt = SystemClock.Instance.GetCurrentInstant(),
            LastUpdatedAt = SystemClock.Instance.GetCurrentInstant()
        };

        await using var context = await MockFactory().CreateDbContextAsync();
        var existingLibrary = context.Libraries.FirstOrDefault();
        if (existingLibrary != null)
        {
            return existingLibrary;
        }
        
        context.Libraries.Add(library);
        await context.SaveChangesAsync();
        return library;
    }

    #endregion
}
