using Melodee.Common.Serialization;
using Melodee.Common.Services.Caching;
using Moq;
using Serilog;

namespace Melodee.Tests.Services.Caching;

public class MemoryCacheManagerTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly ISerializer _serializer;
    private readonly TimeSpan _defaultTimeSpan;
    private readonly MemoryCacheManager _cacheManager;

    public MemoryCacheManagerTests()
    {
        _mockLogger = new Mock<ILogger>();
        _serializer = new Serializer(_mockLogger.Object);
        _defaultTimeSpan = TimeSpan.FromMinutes(10);
        _cacheManager = new MemoryCacheManager(_mockLogger.Object, _defaultTimeSpan, _serializer);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        Assert.Same(_serializer, _cacheManager.Serializer);
    }

    [Fact]
    public async Task Clear_ShouldResetCache()
    {
        // Arrange
        var testData = "test data";
        var key = "test-key";

        // Act - Add item to cache and then clear it
         await _cacheManager.GetAsync(key, () => Task.FromResult(testData), CancellationToken.None);
        _cacheManager.Clear();

        // Re-fetch the item - should cause cache miss and new fetch
        bool wasCalled = false;
        var result = await _cacheManager.GetAsync(key, () =>
        {
            wasCalled = true;
            return Task.FromResult(testData);
        }, CancellationToken.None);

        // Assert
        Assert.True(wasCalled, "Factory method should be called after cache was cleared");
        Assert.Equal(testData, result);
    }

    [Fact]
    public async Task ClearRegion_ShouldClearEntireCache()
    {
        // Arrange
        var testData = "test data";
        var key = "test-key";
        var region = "test-region";

        // Act - Add item to cache with a region and then clear that region
        await _cacheManager.GetAsync(key, () => Task.FromResult(testData), CancellationToken.None, region: region);
        _cacheManager.ClearRegion(region);

        // Re-fetch the item - should cause cache miss and new fetch
        bool wasCalled = false;
        var result = await _cacheManager.GetAsync(key, () =>
        {
            wasCalled = true;
            return Task.FromResult(testData);
        }, CancellationToken.None, region: region);

        // Assert
        Assert.True(wasCalled, "Factory method should be called after region was cleared");
        Assert.Equal(testData, result);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCachedValue_WhenKeyExists()
    {
        // Arrange
        var testData = "test data";
        var key = "test-key";
        int callCount = 0;

        // Act - Call twice with the same key
        var result1 = await _cacheManager.GetAsync(key, () =>
        {
            callCount++;
            return Task.FromResult(testData);
        }, CancellationToken.None);

        var result2 = await _cacheManager.GetAsync(key, () =>
        {
            callCount++;
            return Task.FromResult("different data");
        }, CancellationToken.None);

        // Assert
        Assert.Equal(1, callCount);
        Assert.Equal(testData, result1);
        Assert.Equal(testData, result2); // Should return cached value, not "different data"
    }

    [Fact]
    public async Task GetAsync_ShouldCallFactory_WhenKeyDoesNotExist()
    {
        // Arrange
        var testData = "test data";
        var key = "test-key";
        bool wasCalled = false;

        // Act
        var result = await _cacheManager.GetAsync(key, () =>
        {
            wasCalled = true;
            return Task.FromResult(testData);
        }, CancellationToken.None);

        // Assert
        Assert.True(wasCalled, "Factory method should be called for cache miss");
        Assert.Equal(testData, result);
    }

    [Fact]
    public async Task GetAsync_ShouldRespectCustomDuration()
    {
        // Arrange
        var testData = "test data";
        var key = "test-key";
        var shortDuration = TimeSpan.FromMilliseconds(50);
        int callCount = 0;

        // Act - Get with short duration
        var result1 = await _cacheManager.GetAsync(key, () =>
        {
            callCount++;
            return Task.FromResult(testData);
        }, CancellationToken.None, shortDuration);

        // Wait for the cache entry to expire
        await Task.Delay(100);

        // Get again with the same key
        var result2 = await _cacheManager.GetAsync(key, () =>
        {
            callCount++;
            return Task.FromResult("new data");
        }, CancellationToken.None);

        // Assert
        Assert.Equal(2, callCount);
        Assert.Equal(testData, result1);
        Assert.Equal("new data", result2); // Should get new value after expiration
    }

    [Fact]
    public async Task Remove_ShouldRemoveItemFromCache()
    {
        // Arrange
        var testData = "test data";
        var key = "test-key";
        int callCount = 0;

        // Act - Add to cache
        var result1 = await _cacheManager.GetAsync(key, () =>
        {
            callCount++;
            return Task.FromResult(testData);
        }, CancellationToken.None);

        // Remove from cache
        bool removed = _cacheManager.Remove(key);

        // Try to get again
        var result2 = await _cacheManager.GetAsync(key, () =>
        {
            callCount++;
            return Task.FromResult("new data");
        }, CancellationToken.None);

        // Assert
        Assert.True(removed, "Remove should return true");
        Assert.Equal(2, callCount);
        Assert.Equal(testData, result1);
        Assert.Equal("new data", result2); // Should get new value after removal
    }

    [Fact]
    public async Task Remove_WithRegion_ShouldRemoveItemFromCache()
    {
        // Arrange
        var testData = "test data";
        var key = "test-key";
        var region = "test-region";
        int callCount = 0;

        // Act - Add to cache
        var result1 = await _cacheManager.GetAsync(key, () =>
        {
            callCount++;
            return Task.FromResult(testData);
        }, CancellationToken.None, region: region);

        // Remove from cache
        bool removed = _cacheManager.Remove(key, region);

        // Try to get again
        var result2 = await _cacheManager.GetAsync(key, () =>
        {
            callCount++;
            return Task.FromResult("new data");
        }, CancellationToken.None, region: region);

        // Assert
        Assert.True(removed, "Remove should return true");
        Assert.Equal(2, callCount);
        Assert.Equal(testData, result1);
        Assert.Equal("new data", result2); // Should get new value after removal
    }

    [Fact]
    public async Task GetAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var key = "test-key";
        var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Cancel the token immediately

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _cacheManager.GetAsync(key, async () =>
            {
                // Simulate a task that respects cancellation
                cts.Token.ThrowIfCancellationRequested();
                await Task.Delay(100, cts.Token);
                return "test data";
            }, cts.Token));
    }

    [Fact]
    public async Task GetAsync_ShouldWorkWithComplexTypes()
    {
        // Arrange
        var testData = new TestClass { Id = 1, Name = "Test" };
        var key = "test-key";

        // Act
        var result = await _cacheManager.GetAsync(key, () => Task.FromResult(testData), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
    }

    private class TestClass
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
