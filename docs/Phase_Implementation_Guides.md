# Phase-Specific Implementation Guides for Independent Copilot Sessions

## 📋 IMPORTANT: How to Use This Document for Independent Copilot Sessions

Each phase is designed to be executed in a **completely independent** Copilot session. To start any phase:

1. **Copy the entire "Copilot Session Context" section** for your target phase
2. **Paste it at the beginning of a new Copilot session**
3. **Follow the implementation checklist** step by step
4. **Mark checkboxes as you complete each item**
5. **Validate all success criteria** before considering the phase complete

### ⚠️ Critical Notes for Session Independence
- Each phase context includes ALL necessary information for that phase
- No need to reference other phases during implementation
- File paths are absolute and specific
- Interface signatures are complete and ready to implement
- Performance requirements are specific and measurable

## How to Use This Document

Each phase is designed to be executed in an independent Copilot session. Each phase contains:

1. **Quick Start Guide** - Context for Copilot to understand the project
2. **Current State Analysis** - What exists and what needs to be extracted
3. **Specific Implementation Requirements** - Exact interfaces and method signatures
4. **File Paths and Structure** - Where to create/modify files
5. **Performance Requirements** - Specific targets for high-throughput service
6. **Testing Requirements** - What tests must be created
7. **Success Criteria** - Checkboxes to validate completion
8. **Implementation Checklist** - Detailed step-by-step progress tracking

## Before Starting Any Phase

### Prerequisite Information for Copilot
```
PROJECT: Melodee Music Server - High-performance OpenSubsonic API
ARCHITECTURE: Domain-Driven Design (DDD) with performance optimization
THROUGHPUT: Service handles 1000+ requests/second
TECHNOLOGY: .NET 9, Entity Framework Core, MemoryCache caching

WORKSPACE: /home/steven/source/melodee/
MAIN SERVICE: src/Melodee.Common/Services/OpenSubsonicApiService.cs (3,771 lines)
TESTS: tests/Melodee.Common.Tests/

PERFORMANCE TARGETS:
- Authentication: <10ms P99
- Image operations: <50ms cached, <200ms processed
- Search operations: <100ms P99
- Database queries: Use compiled queries for hot paths
```

### Overall Project Progress Tracking
- [ ] Phase 1: Authentication and Authorization Infrastructure ⭐ **START HERE**
- [ ] Phase 2: Media and Image Operations (Requires Phase 1)
- [ ] Phase 3: Search and Discovery Operations (Requires Phase 1)
- [ ] Phase 4: Playlist Management Operations (Requires Phases 1-3)
- [ ] Phase 5: Sharing Operations (Requires Phases 1-3)
- [ ] Phase 6: User Activity Tracking (Requires Phases 1-3)
- [ ] Phase 7: System Operations (Requires Phases 1-3)
- [ ] Phase 8: Final OpenSubsonicApiService Transformation (Requires ALL previous phases)

### 🎯 Dependencies and Parallel Execution
```
Phase 1 (Authentication) ← Must be completed first
    ↓
Phases 2, 3 (Media, Search) ← Can be done in parallel after Phase 1
    ↓
Phases 4, 5, 6, 7 (Business Logic) ← Can be done in parallel after Phases 1-3
    ↓
Phase 8 (Final Cleanup) ← Must be done last
```

### Validation Steps After Each Phase
- [ ] Run existing tests to ensure no breaking changes
- [ ] Run new performance tests to validate targets
- [ ] Check code coverage meets requirements (90%+ domain, 80%+ adapters)
- [ ] Validate no business logic remains in OpenSubsonicApiService for completed areas

---

## Phase 1: Authentication and Authorization Infrastructure

### Copilot Session Context for Phase 1

#### Quick Start Guide for Copilot
```markdown
PROJECT: Melodee Music Server - OpenSubsonic API refactoring
CURRENT PHASE: Phase 1 - Authentication Infrastructure
GOAL: Extract authentication logic from OpenSubsonicApiService into proper DDD structure

KEY FILES:
- Source: /home/steven/source/melodee/src/Melodee.Common/Services/OpenSubsonicApiService.cs (3,771 lines)
- Enhance: /home/steven/source/melodee/src/Melodee.Common/Services/UserService.cs
- Create: /home/steven/source/melodee/src/Melodee.Common/Services/Infrastructure/IAuthenticationService.cs
- Create: /home/steven/source/melodee/src/Melodee.Common/Services/Adapters/OpenSubsonicAuthenticationAdapter.cs

PERFORMANCE REQUIREMENTS:
- Authentication must complete in <10ms
- Support 1000+ concurrent requests
- Use compiled queries for database access
```

#### Current State Analysis
**Authentication Methods in OpenSubsonicApiService to Extract:**
- `AuthenticateSubsonicApiAsync()` - Currently ~100 lines at line 850-950
- Token validation logic - Currently ~100 lines at line 1200-1300
- Rate limiting checks - Currently ~50 lines at line 200-250
- User permission validation - Currently ~50 lines at line 300-350

#### Specific Implementation Requirements

**1. IAuthenticationService Interface**
```csharp
// File: /src/Melodee.Common/Services/Infrastructure/IAuthenticationService.cs
namespace Melodee.Common.Services.Infrastructure;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<AuthenticationResult> RefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> CheckUserPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);
    Task<bool> CanUserShareAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CanUserAdministerAsync(Guid userId, CancellationToken cancellationToken = default);
}

public record AuthenticationResult(string Token, DateTime Expiration);
```

**2. Resilience Service Interface**
```csharp
// File: /src/Melodee.Common/Services/Infrastructure/IResilienceService.cs
namespace Melodee.Common.Services.Infrastructure;

public interface IResilienceService
{
    Task<TReturn> ExecuteWithCircuitBreakerAsync<TReturn>(Func<Task<TReturn>> action, CancellationToken cancellationToken = default);
    Task ExecuteWithRateLimitingAsync(Func<Task> action, CancellationToken cancellationToken = default);
}
```

#### Phase 1 Implementation Checklist

##### Infrastructure Services Creation
- [ ] Create `IAuthenticationService.cs` interface with complete method signatures
- [ ] Create `AuthenticationService.cs` implementation
- [ ] Add `IResilienceService.cs` interface for circuit breaker and rate limiting
- [ ] Create `ResilienceService.cs` implementation
- [ ] Add dependency injection registrations in `ServiceCollectionExtensions.cs`

##### Domain Service Enhancements
- [ ] Add compiled query `AuthenticateUserQuery` to `UserService.cs`
- [ ] Add `ValidateUserCredentialsAsync()` method to `UserService.cs`
- [ ] Add `ValidateUserTokenAsync()` method to `UserService.cs`
- [ ] Add `CheckUserPermissionAsync()` method to `UserService.cs`
- [ ] Add `CanUserShareAsync()` method to `UserService.cs`
- [ ] Add `CanUserAdministerAsync()` method to `UserService.cs`

##### API Adapter Creation
- [ ] Create `OpenSubsonicAuthenticationAdapter.cs` with thin wrapper methods
- [ ] Implement `AuthenticateSubsonicApiAsync()` method
- [ ] Add OpenSubsonic-specific error formatting
- [ ] Add API-specific response mapping
- [ ] Implement rate limiting per IP/user

##### Performance Optimizations
- [ ] Implement compiled queries for authentication (<5ms target)
- [ ] Add user permission caching (5-minute expiry)
- [ ] Implement in-memory sliding window rate limiting
- [ ] Add async security event logging
- [ ] Optimize database connection pooling

##### Code Extraction from OpenSubsonicApiService
- [ ] Extract `AuthenticateSubsonicApiAsync()` method (lines ~850-950)
- [ ] Extract token validation logic (lines ~1200-1300)
- [ ] Extract rate limiting checks (lines ~200-250)
- [ ] Extract user permission validation (lines ~300-350)
- [ ] Remove all extracted authentication code from OpenSubsonicApiService
- [ ] Update OpenSubsonicApiService to use new authentication adapter

##### Testing Implementation
- [ ] Create `AuthenticationServiceTests.cs` with unit tests
- [ ] Create `ResilienceServiceTests.cs` with unit tests
- [ ] Enhance `UserServiceTests.cs` with new authentication method tests
- [ ] Create `OpenSubsonicAuthenticationAdapterTests.cs` with adapter tests
- [ ] Create performance test for <10ms authentication requirement
- [ ] Create load test for 1000+ concurrent authentication requests
- [ ] Create rate limiting tests
- [ ] Create circuit breaker tests

##### Validation and Cleanup
- [ ] Run all existing tests to ensure no breaking changes
- [ ] Validate authentication performance <10ms P99
- [ ] Check code coverage meets requirements (90%+ domain, 80%+ adapters)
- [ ] Validate rate limiting works under load
- [ ] Confirm all authentication logic removed from API layer
- [ ] Update documentation for new authentication methods

**📋 Note: Check all boxes above before considering Phase 1 complete. Each checkbox represents a specific deliverable that must be implemented and tested.**

---

## Phase 2: Media and Image Operations

### Copilot Session Context for Phase 2

#### Quick Start Guide for Copilot
```markdown
PROJECT: Melodee Music Server - OpenSubsonic API refactoring
CURRENT PHASE: Phase 2 - Media and Image Infrastructure
GOAL: Extract image/media operations from OpenSubsonicApiService into proper DDD structure

DEPENDENCIES: Phase 1 must be completed (authentication infrastructure)

KEY FILES:
- Source: /home/steven/source/melodee/src/Melodee.Common/Services/OpenSubsonicApiService.cs
- Enhance: /home/steven/source/melodee/src/Melodee.Common/Services/AlbumService.cs
- Enhance: /home/steven/source/melodee/src/Melodee.Common/Services/ArtistService.cs
- Enhance: /home/steven/source/melodee/src/Melodee.Common/Services/UserService.cs
- Create: /home/steven/source/melodee/src/Melodee.Common/Services/Infrastructure/IImageProcessingService.cs
- Create: /home/steven/source/melodee/src/Melodee.Common/Services/Infrastructure/IMediaStreamingService.cs
- Create: /home/steven/source/melodee/src/Melodee.Common/Services/Adapters/OpenSubsonicMediaAdapter.cs

PERFORMANCE REQUIREMENTS:
- Cached images: <50ms response time
- Processed images: <200ms response time
- Use multi-layer caching (L1 in-memory, L2 Redis)
- Implement object pooling for byte arrays
```

#### Current State Analysis
**Image/Media Methods in OpenSubsonicApiService to Extract:**
- `GetCoverArtAsync()` - Currently ~150 lines at line 1500-1650
- `GetAvatarAsync()` - Currently ~80 lines at line 1700-1780
- `StreamAsync()` - Currently ~200 lines at line 2000-2200
- Image resizing logic scattered throughout
- Direct file system access mixed with business logic

#### Specific Implementation Requirements

**1. IImageProcessingService Interface**
```csharp
// File: /src/Melodee.Common/Services/Infrastructure/IImageProcessingService.cs
namespace Melodee.Common.Services.Infrastructure;

public interface IImageProcessingService
{
    Task<ProcessedImage> ResizeImageAsync(byte[] imageData, int targetWidth, int targetHeight, CancellationToken cancellationToken = default);
    Task<ProcessedImage> ResizeImageAsync(Stream imageStream, int targetWidth, int targetHeight, CancellationToken cancellationToken = default);
    Task<byte[]> ConvertImageFormatAsync(byte[] imageData, ImageFormat targetFormat, CancellationToken cancellationToken = default);
    Task<ImageMetadata> GetImageMetadataAsync(byte[] imageData, CancellationToken cancellationToken = default);
    bool IsValidImageSize(int width, int height);
    string GetContentTypeForFormat(ImageFormat format);
}

public record ProcessedImage(byte[] Data, ImageFormat Format, int Width, int Height, string ContentType);
public record ImageMetadata(int Width, int Height, ImageFormat Format, long SizeInBytes);
public enum ImageFormat { Jpeg, Png, WebP, Gif }
```

**2. IMediaStreamingService Interface**
```csharp
// File: /src/Melodee.Common/Services/Infrastructure/IMediaStreamingService.cs
namespace Melodee.Common.Services.Infrastructure;

public interface IMediaStreamingService
{
    Task<MediaStream> GetAudioStreamAsync(string filePath, AudioFormat? targetFormat = null, int? bitrate = null, CancellationToken cancellationToken = default);
    Task<MediaStream> GetVideoStreamAsync(string filePath, VideoFormat? targetFormat = null, CancellationToken cancellationToken = default);
    Task<MediaMetadata> GetMediaMetadataAsync(string filePath, CancellationToken cancellationToken = default);
    bool SupportsFormat(string fileExtension);
    string GetContentTypeForFormat(AudioFormat format);
}

public record MediaStream(Stream Data, string ContentType, long ContentLength, MediaMetadata Metadata);
public record MediaMetadata(TimeSpan Duration, int Bitrate, string Format, long SizeInBytes);
public enum AudioFormat { Mp3, Flac, Ogg, M4a, Wav }
public enum VideoFormat { Mp4, Mkv, Avi, WebM }
```

**3. AlbumService Enhancements**
```csharp
// Add these methods to existing AlbumService class
// File: /src/Melodee.Common/Services/AlbumService.cs

public async Task<OperationResult<ProcessedImage>> GetAlbumCoverArtAsync(
    Guid albumId,
    int? size = null,
    CancellationToken cancellationToken = default)
{
    // Use multi-layer caching: L1 (memory) -> L2 (Redis) -> Database/FileSystem
    // Implementation details in phase execution
}

public async Task<OperationResult<Stream>> GetAlbumCoverArtStreamAsync(
    Guid albumId,
    int? size = null,
    CancellationToken cancellationToken = default)
{
    // Stream large images without loading entirely into memory
    // Implementation details in phase execution
}

public async Task<OperationResult<bool>> SetAlbumCoverArtAsync(
    Guid albumId,
    byte[] imageData,
    CancellationToken cancellationToken = default)
{
    // Validate, process, and store album cover art
    // Implementation details in phase execution
}
```

**4. Performance-Critical Caching Strategy**
```csharp
// Enhanced caching for high-performance image serving
public class AlbumService
{
    private readonly IPerformanceCacheService _performanceCache;
    private readonly IImageProcessingService _imageProcessing;
    private readonly ArrayPool<byte> _byteArrayPool = ArrayPool<byte>.Shared;

    public async Task<ProcessedImage?> GetAlbumCoverArtAsync(Guid albumId, int? size = null)
    {
        var cacheKey = $"album:cover:{albumId}:{size ?? 0}";

        // L1 Cache (microsecond access)
        var cachedImage = await _performanceCache.GetHotAsync<ProcessedImage>(cacheKey);
        if (cachedImage != null) return cachedImage;

        // L2 Cache (millisecond access)
        cachedImage = await _performanceCache.GetWarmAsync<ProcessedImage>(cacheKey);
        if (cachedImage != null)
        {
            // Promote to L1
            await _performanceCache.SetHotAsync(cacheKey, cachedImage, TimeSpan.FromMinutes(10));
            return cachedImage;
        }

        // Database/FileSystem (only if not cached)
        var rawImage = await GetRawAlbumImageAsync(albumId);
        if (rawImage == null) return null;

        // Process image using object pooling
        var processedImage = size.HasValue
            ? await _imageProcessing.ResizeImageAsync(rawImage, size.Value, size.Value)
            : new ProcessedImage(rawImage, ImageFormat.Jpeg, 0, 0, "image/jpeg");

        // Cache in both layers
        await _performanceCache.SetHotAsync(cacheKey, processedImage, TimeSpan.FromMinutes(10));
        await _performanceCache.SetWarmAsync(cacheKey, processedImage, TimeSpan.FromHours(6));

        return processedImage;
    }
}
```

#### Methods to Extract from OpenSubsonicApiService
1. **Line ~1500-1650**: `GetCoverArtAsync()` method
2. **Line ~1700-1780**: `GetAvatarAsync()` method
3. **Line ~2000-2200**: `StreamAsync()` method
4. **Line ~1800-1900**: Image validation and processing logic
5. **Line ~2300-2400**: Media format detection logic

#### Performance Requirements for Phase 2
- **Image Caching**: L1 (10-minute expiry), L2 (6-hour expiry)
- **Object Pooling**: Use ArrayPool<byte> for image processing buffers
- **Memory Management**: Limit concurrent image processing to prevent OOM
- **Streaming**: Support range requests for large media files
- **Compression**: Use WebP format when supported by client

#### Testing Requirements for Phase 2
```csharp
// Required test files to create:
// tests/Melodee.Common.Tests/Services/Infrastructure/ImageProcessingServiceTests.cs
// tests/Melodee.Common.Tests/Services/Infrastructure/MediaStreamingServiceTests.cs
// tests/Melodee.Common.Tests/Services/AlbumServiceTests.cs (enhance existing)
// tests/Melodee.Common.Tests/Services/Adapters/OpenSubsonicMediaAdapterTests.cs

// Performance test requirements:
[Fact]
public async Task GetAlbumCoverArt_CachedImage_ShouldComplete_Within50ms()
{
    // Load test for cached images
}

[Fact]
public async Task GetAlbumCoverArt_ImageProcessing_ShouldComplete_Within200ms()
{
    // Load test for image processing
}

[Theory]
[InlineData(100, 100)]
[InlineData(500, 500)]
[InlineData(1000, 1000)]
public async Task ResizeImage_VariousSizes_ShouldMaintainQuality(int width, int height)
{
    // Quality and performance test
}
```

#### Success Criteria for Phase 2
- [ ] All image/media logic removed from OpenSubsonicApiService
- [ ] Multi-layer caching implemented and tested
- [ ] Image processing performance <200ms P99
- [ ] Cached image serving <50ms P99
- [ ] Memory usage stable under load (no leaks)
- [ ] All tests passing with >90% coverage

---

## Phase 3: Search and Discovery Operations

### Copilot Session Context for Phase 3

#### Quick Start Guide for Copilot
```markdown
PROJECT: Melodee Music Server - OpenSubsonic API refactoring
CURRENT PHASE: Phase 3 - Search and Discovery Infrastructure
GOAL: Extract search operations from OpenSubsonicApiService into proper DDD structure

DEPENDENCIES: Phase 1 (authentication) and Phase 2 (media) must be completed

KEY FILES:
- Source: /home/steven/source/melodee/src/Melodee.Common/Services/OpenSubsonicApiService.cs
- Enhance: /home/steven/source/melodee/src/Melodee.Common/Services/ArtistService.cs
- Enhance: /home/steven/source/melodee/src/Melodee.Common/Services/AlbumService.cs
- Enhance: /home/steven/source/melodee/src/Melodee.Common/Services/SongService.cs
- Enhance: /home/steven/source/melodee/src/Melodee.Common/Services/LibraryService.cs
- Create: /home/steven/source/melodee/src/Melodee.Common/Services/Infrastructure/ISearchIndexService.cs
- Create: /home/steven/source/melodee/src/Melodee.Common/Services/Adapters/OpenSubsonicSearchAdapter.cs

PERFORMANCE REQUIREMENTS:
- Search operations: <100ms P99
- Use compiled queries for frequent searches
- Implement search result caching
- Support pagination for large result sets
```

#### Current State Analysis
**Search Methods in OpenSubsonicApiService to Extract:**
- `Search2Async()` - Currently ~200 lines at line 2500-2700
- `Search3Async()` - Currently ~250 lines at line 2750-3000
- `GetIndexesAsync()` - Currently ~100 lines at line 3100-3200
- `GetMusicDirectoryAsync()` - Currently ~180 lines at line 3250-3430
- Artist/Album/Song search logic scattered throughout

#### Specific Implementation Requirements

**1. ISearchIndexService Interface**
```csharp
// File: /src/Melodee.Common/Services/Infrastructure/ISearchIndexService.cs
namespace Melodee.Common.Services.Infrastructure;

public interface ISearchIndexService
{
    Task<SearchResults<T>> SearchAsync<T>(SearchQuery query, CancellationToken cancellationToken = default) where T : class;
    Task<SearchResults<T>> SearchAsync<T>(string searchTerm, SearchOptions options, CancellationToken cancellationToken = default) where T : class;
    Task IndexEntityAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;
    Task RemoveFromIndexAsync<T>(Guid entityId, CancellationToken cancellationToken = default) where T : class;
    Task<bool> IsIndexHealthyAsync(CancellationToken cancellationToken = default);
    Task RebuildIndexAsync<T>(CancellationToken cancellationToken = default) where T : class;
}

public record SearchQuery(string Term, int Offset, int Count, SearchFilter[] Filters, SearchSort[] Sorts);
public record SearchResults<T>(IEnumerable<T> Items, int TotalCount, int Offset, int Count, TimeSpan SearchTime);
public record SearchOptions(int Offset = 0, int Count = 50, bool IncludeScore = false);
public record SearchFilter(string Field, SearchFilterOperator Operator, object Value);
public record SearchSort(string Field, SearchSortDirection Direction);

public enum SearchFilterOperator { Equals, Contains, StartsWith, GreaterThan, LessThan, Range }
public enum SearchSortDirection { Ascending, Descending }
```

**2. Enhanced ArtistService for Search**
```csharp
// Add these methods to existing ArtistService class
// File: /src/Melodee.Common/Services/ArtistService.cs

// Use compiled query for performance
private static readonly Func<MelodeeDbContext, string, int, int, Task<List<Artist>>>
    SearchArtistsQuery = EF.CompileAsyncQuery(
        (MelodeeDbContext context, string searchTerm, int offset, int count) =>
            context.Artists
                .Where(a => a.Name.Contains(searchTerm) || a.SortName.Contains(searchTerm))
                .OrderBy(a => a.SortName)
                .Skip(offset)
                .Take(count)
                .ToList());

public async Task<OperationResult<SearchResults<Artist>>> SearchArtistsAsync(
    string query,
    PaginationRequest pagination,
    CancellationToken cancellationToken = default)
{
    // High-performance search with caching
    // Implementation details in phase execution
}

public async Task<OperationResult<IEnumerable<ArtistIndex>>> GetArtistIndexesAsync(
    CancellationToken cancellationToken = default)
{
    // Optimized alphabetical index generation
    // Implementation details in phase execution
}

public async Task<OperationResult<IEnumerable<Artist>>> GetArtistsByLetterAsync(
    char letter,
    PaginationRequest pagination,
    CancellationToken cancellationToken = default)
{
    // Optimized letter-based filtering
    // Implementation details in phase execution
}
```

#### Performance Requirements for Phase 3
- **Search Caching**: Cache search results for 5 minutes
- **Database Optimization**: Use compiled queries and proper indexes
- **Pagination**: Efficient offset/limit queries with total count caching
- **Index Management**: Background index rebuilding for search performance

#### Testing Requirements for Phase 3
```csharp
// Required test files to create:
// tests/Melodee.Common.Tests/Services/Infrastructure/SearchIndexServiceTests.cs
// tests/Melodee.Common.Tests/Services/ArtistServiceTests.cs (enhance existing)
// tests/Melodee.Common.Tests/Services/AlbumServiceTests.cs (enhance existing)
// tests/Melodee.Common.Tests/Services/SongServiceTests.cs (enhance existing)
// tests/Melodee.Common.Tests/Services/LibraryServiceTests.cs (enhance existing)
// tests/Melodee.Common.Tests/Services/Adapters/OpenSubsonicSearchAdapterTests.cs

// Performance test requirements:
[Fact]
public async Task SearchArtists_ValidQuery_ShouldComplete_Within100ms()
{
    // Load test for artist search
}

[Fact]
public async Task SearchAlbums_ValidQuery_ShouldComplete_Within100ms()
{
    // Load test for album search
}

[Fact]
public async Task SearchSongs_ValidQuery_ShouldComplete_Within100ms()
{
    // Load test for song search
}

[Fact]
public async Task GetArtistIndexes_ShouldReturn_OrderedList()
{
    // Test artist index retrieval
}

[Fact]
public async Task GetArtistsByLetter_ShouldReturn_CorrectArtists()
{
    // Test artist retrieval by letter
}
```

#### Success Criteria for Phase 3
- [ ] All search logic removed from OpenSubsonicApiService
- [ ] Search operations <100ms P99
- [ ] Compiled queries implemented for hot search paths
- [ ] Search result caching working effectively
- [ ] All tests passing with >90% coverage

---

## Using This Document for Independent Sessions

### For Each Phase Session:

1. **Copy the specific phase context** to the beginning of your Copilot session
2. **Reference the file paths** provided for exact implementation locations
3. **Follow the specific implementation requirements** with provided interfaces
4. **Validate success criteria** before considering the phase complete
5. **Run the suggested tests** to ensure performance and functionality

### Phase Dependencies:
- **Phase 1**: Independent (can start immediately)
- **Phase 2**: Requires Phase 1 completion
- **Phase 3**: Requires Phase 1 completion
- **Phase 4-8**: Can be done in parallel after Phase 1-3 are complete

This structure ensures each Copilot session has complete context and specific implementation guidance without requiring knowledge of other phases.
