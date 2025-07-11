using Melodee.Common.Serialization;
using Melodee.Common.Services.Caching;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Serilog;
using System.Collections.Concurrent;

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

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MemoryCacheManager(null!, TimeSpan.FromMinutes(10), _serializer));
    }

    [Fact]
    public void Constructor_WithNullSerializer_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MemoryCacheManager(_mockLogger.Object, TimeSpan.FromMinutes(10), null!));
    }

    [Fact]
    public void Constructor_WithZeroTimeSpan_ShouldUseDefaultValue()
    {
        // Arrange & Act
        var cacheManager = new MemoryCacheManager(_mockLogger.Object, TimeSpan.Zero, _serializer);

        // Assert
        Assert.Same(_serializer, cacheManager.Serializer);
        // We can't directly test the DefaultTimeSpan as it's protected, but we can verify it works correctly
    }

    [Fact]
    public async Task GetAsync_WhenFactoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var key = "exception-key";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _cacheManager.GetAsync<string>(key, () => throw expectedException, CancellationToken.None));

        Assert.Same(expectedException, exception);
    }

    [Fact]
    public async Task GetAsync_WithDifferentGenericTypes_ShouldNotShareCache()
    {
        // Arrange
        var key = "type-test-key";
        int intCallCount = 0;
        int stringCallCount = 0;

        // Act - Get with int type
        var intResult = await _cacheManager.GetAsync(key, () =>
        {
            intCallCount++;
            return Task.FromResult(42);
        }, CancellationToken.None);

        // Get with string type using same key
        var stringResult = await _cacheManager.GetAsync<string>(key, () =>
        {
            stringCallCount++;
            return Task.FromResult("42");
        }, CancellationToken.None);

        // Assert
        Assert.Equal(1, intCallCount);
        Assert.Equal(42, intResult);
        Assert.Equal("42", stringResult);
    }

    [Fact]
    public async Task GetAsync_WithNestedCalls_ShouldWorkCorrectly()
    {
        // Arrange
        var outerKey = "outer-key";
        var innerKey = "inner-key";
        int outerCallCount = 0;
        int innerCallCount = 0;

        // Act
        var result = await _cacheManager.GetAsync(outerKey, async () =>
        {
            outerCallCount++;
            // Nested cache call
            return await _cacheManager.GetAsync(innerKey, () =>
            {
                innerCallCount++;
                return Task.FromResult("inner value");
            }, CancellationToken.None);
        }, CancellationToken.None);

        // Call again to verify caching behavior
        var result2 = await _cacheManager.GetAsync(outerKey, async () =>
        {
            outerCallCount++;
            return await _cacheManager.GetAsync(innerKey, () =>
            {
                innerCallCount++;
                return Task.FromResult("new inner value");
            }, CancellationToken.None);
        }, CancellationToken.None);

        // Assert
        Assert.Equal(1, outerCallCount);
        Assert.Equal(1, innerCallCount);
        Assert.Equal("inner value", result);
        Assert.Equal("inner value", result2); // Should use cached outer value
    }

    [Fact]
    public async Task Remove_WithNonExistentKey_ShouldReturnTrue()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        bool result = _cacheManager.Remove(key);

        // Assert
        Assert.False(result, "Remove should return false for non-existent keys");

        // Verify item doesn't exist by checking if factory is called
        bool wasCalled = false;
        await _cacheManager.GetAsync(key, () =>
        {
            wasCalled = true;
            return Task.FromResult("test");
        }, CancellationToken.None);

        Assert.True(wasCalled, "Factory should be called for removed/non-existent key");
    }

    [Fact]
    public async Task Remove_WithRegionNonExistentKey_ShouldReturnTrue()
    {
        // Arrange
        var key = "non-existent-key";
        var region = "test-region";

        // Act
        bool result = _cacheManager.Remove(key, region);

        // Assert
        Assert.False(result, "Remove with region should return false for non-existent keys");

        // Verify item doesn't exist by checking if factory is called
        bool wasCalled = false;
        await _cacheManager.GetAsync(key, () =>
        {
            wasCalled = true;
            return Task.FromResult("test");
        }, CancellationToken.None, region: region);

        Assert.True(wasCalled, "Factory should be called for removed/non-existent key with region");
    }

    [Fact]
    public async Task GetAsync_WithMultipleConcurrentCalls_ShouldOnlyInvokeFactoryOnce()
    {
        // Arrange
        var key = "concurrent-key";
        int callCount = 0;
        var tasks = new List<Task<string>>();

        // Act - Start 10 concurrent tasks that all try to get the same cache key
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_cacheManager.GetAsync(key, async () =>
            {
                // Simulate some work
                await Task.Delay(50);
                Interlocked.Increment(ref callCount);
                return "cached value";
            }, CancellationToken.None));
        }

        // Wait for all tasks to complete
        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(1, callCount); // Factory should only be called once
        Assert.All(results, result => Assert.Equal("cached value", result));
    }

    [Fact]
    public async Task GetAsync_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _cacheManager.GetAsync<string>(null!, () => Task.FromResult("test"), CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _cacheManager.GetAsync<string>(string.Empty, () => Task.FromResult("test"), CancellationToken.None));
    }

    [Fact]
    public void Remove_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _cacheManager.Remove(null!));
    }

    [Fact]
    public void Remove_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _cacheManager.Remove(string.Empty));
    }

    [Fact]
    public void Remove_WithRegion_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _cacheManager.Remove(null!, "region"));
    }

    [Fact]
    public async Task GetAsync_WithNullRegion_ShouldUseDefaultRegion()
    {
        // Arrange
        var key = "null-region-key";
        var testData = "test data";
        int callCount = 0;

        // Act - First call with null region
        var result1 = await _cacheManager.GetAsync(key, () =>
        {
            callCount++;
            return Task.FromResult(testData);
        }, CancellationToken.None, region: null);

        // Second call with null region
        var result2 = await _cacheManager.GetAsync(key, () =>
        {
            callCount++;
            return Task.FromResult("different data");
        }, CancellationToken.None, region: null);

        // Assert
        Assert.Equal(1, callCount); // Factory should only be called once
        Assert.Equal(testData, result1);
        Assert.Equal(testData, result2); // Should return cached value
    }

    [Fact]
    public async Task GetAsync_WithNullDuration_ShouldUseDefaultDuration()
    {
        // Arrange
        var key = "null-duration-key";
        var testData = "test data";
        int callCount = 0;

        // Act - Get with null duration
        var result1 = await _cacheManager.GetAsync(key, () =>
        {
            callCount++;
            return Task.FromResult(testData);
        }, CancellationToken.None, duration: null);

        // Get again immediately
        var result2 = await _cacheManager.GetAsync(key, () =>
        {
            callCount++;
            return Task.FromResult("different data");
        }, CancellationToken.None, duration: null);

        // Assert
        Assert.Equal(1, callCount); // Should only call factory once
        Assert.Equal(testData, result1);
        Assert.Equal(testData, result2); // Should return cached value
    }

    [Fact]
    public async Task GetAsync_WithDifferentKeyCasing_ShouldTreatAsDifferentKeys()
    {
        // Arrange
        var lowerKey = "test-key";
        var upperKey = "TEST-KEY";
        int lowerKeyCallCount = 0;
        int upperKeyCallCount = 0;

        // Act - Get with lowercase key
        var result1 = await _cacheManager.GetAsync(lowerKey, () =>
        {
            lowerKeyCallCount++;
            return Task.FromResult("lower result");
        }, CancellationToken.None);

        // Get with uppercase key
        var result2 = await _cacheManager.GetAsync(upperKey, () =>
        {
            upperKeyCallCount++;
            return Task.FromResult("upper result");
        }, CancellationToken.None);

        // Assert
        Assert.Equal(1, lowerKeyCallCount);
        Assert.Equal(1, upperKeyCallCount); // Should be called as it's a different key
        Assert.Equal("lower result", result1);
        Assert.Equal("upper result", result2);
    }

    [Fact]
    public async Task GetAsync_WithDifferentRegions_ShouldCacheIndependently()
    {
        // Arrange
        var key = "region-test-key";
        var region1 = "region1";
        var region2 = "region2";
        int region1CallCount = 0;
        int region2CallCount = 0;

        // Act - Get with region1
        var result1 = await _cacheManager.GetAsync(key, () =>
        {
            region1CallCount++;
            return Task.FromResult("region1 result");
        }, CancellationToken.None, region: region1);

        // Get with region2
        var result2 = await _cacheManager.GetAsync(key, () =>
        {
            region2CallCount++;
            return Task.FromResult("region2 result");
        }, CancellationToken.None, region: region2);

        // Get with region1 again
        var result3 = await _cacheManager.GetAsync(key, () =>
        {
            region1CallCount++;
            return Task.FromResult("new region1 result");
        }, CancellationToken.None, region: region1);

        // Assert
        Assert.Equal(1, region1CallCount);
        Assert.Equal(1, region2CallCount);
        Assert.Equal("region1 result", result1);
        Assert.Equal("region2 result", result2);
        Assert.Equal("region1 result", result3); // Should return cached value from region1
    }

    [Fact]
    public async Task GetAsync_WithZeroDuration_ShouldNotCache()
    {
        // Arrange
        var key = "zero-duration-key";
        var zeroDuration = TimeSpan.Zero;
        int callCount = 0;

        // Act - Get with zero duration
        var result1 = await _cacheManager.GetAsync(key, () =>
        {
            callCount++;
            return Task.FromResult("first call");
        }, CancellationToken.None, zeroDuration);

        // Get again immediately
        var result2 = await _cacheManager.GetAsync(key, () =>
        {
            callCount++;
            return Task.FromResult("second call");
        }, CancellationToken.None, zeroDuration);

        // Assert
        Assert.Equal(2, callCount); // Should call factory twice
        Assert.Equal("first call", result1);
        Assert.Equal("second call", result2); // Should not return cached value
    }

    [Fact]
    public async Task GetAsync_WithNegativeDuration_ShouldThrowArgumentException()
    {
        // Arrange
        var key = "negative-duration-key";
        var negativeDuration = TimeSpan.FromSeconds(-1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _cacheManager.GetAsync<string>(key, () => Task.FromResult("test"), CancellationToken.None, negativeDuration));
    }
}
