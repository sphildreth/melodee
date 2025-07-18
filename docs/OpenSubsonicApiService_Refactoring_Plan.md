# OpenSubsonicApiService Refactoring Plan (DDD-Aligned)

## Overview
The OpenSubsonicApiService class is currently 3,771 lines long and violates both SOLID principles and Domain-Driven Design (DDD) patterns. Instead of creating OpenSubsonic-specific services that duplicate domain logic, this refactoring plan follows DDD best practices by enhancing existing domain services and creating thin API adapters.

## Domain-Driven Design Principles Applied

### Core Concepts
1. **Domain Services** (UserService, AlbumService, etc.) should contain ALL business logic for their domain
2. **Application Services** should be thin coordinators that handle protocol-specific concerns
3. **Infrastructure Services** should handle cross-cutting technical concerns
4. **No domain logic duplication** across service boundaries

### Current Anti-Patterns
1. **Business Logic in API Layer**: OpenSubsonicApiService contains domain logic that belongs in domain services
2. **Direct Database Access**: Bypassing domain services for data operations
3. **Protocol-Specific Domain Logic**: Mixing OpenSubsonic API concerns with business operations
4. **Scattered Responsibilities**: Authentication, caching, and business logic mixed together

## Refactoring Strategy
Enhance existing domain services with missing functionality while extracting thin API adapters and infrastructure services.

---

## Phase 1: Extract Authentication and Authorization Infrastructure
**Files to Create**: 2 files
**Files to Enhance**: 1 domain service
**Lines to Refactor**: ~200-300 lines

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
- Token validation logic scattered throughout
- Rate limiting mixed with business logic
- Direct database queries in API layer

#### Specific Implementation Requirements

**1. IAuthenticationService Interface**
```csharp
// File: /src/Melodee.Common/Services/Infrastructure/IAuthenticationService.cs
namespace Melodee.Common.Services.Infrastructure;

public interface IAuthenticationService
{
    Task<AuthenticationResult> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<AuthenticationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> CheckRateLimitAsync(string identifier, int maxAttempts, TimeSpan window, CancellationToken cancellationToken = default);
    Task LogSecurityEventAsync(SecurityEvent securityEvent, CancellationToken cancellationToken = default);
}

public record AuthenticationResult(bool IsSuccess, Guid? UserId, string? ErrorCode, TimeSpan Duration);
public record SecurityEvent(string Type, string Identifier, string Details, DateTimeOffset Timestamp);
```

**2. UserService Enhancements**
```csharp
// Add these methods to existing UserService class
// File: /src/Melodee.Common/Services/UserService.cs

// Use compiled query for performance (must be static)
private static readonly Func<MelodeeDbContext, string, string, Task<User?>>
    AuthenticateUserQuery = EF.CompileAsyncQuery(
        (MelodeeDbContext context, string username, string passwordHash) =>
            context.Users
                .Where(u => u.Username == username && u.PasswordHash == passwordHash && u.IsActive)
                .FirstOrDefault());

public async Task<OperationResult<UserAuthInfo>> ValidateUserCredentialsAsync(
    string username,
    string password,
    CancellationToken cancellationToken = default)
{
    // Implementation details in phase execution
}

public async Task<OperationResult<bool>> CheckUserPermissionAsync(
    Guid userId,
    Permission permission,
    CancellationToken cancellationToken = default)
{
    // Implementation details in phase execution
}

public async Task<OperationResult<bool>> CanUserShareAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
{
    // Implementation details in phase execution
}
```

**3. OpenSubsonicAuthenticationAdapter**
```csharp
// File: /src/Melodee.Common/Services/Adapters/OpenSubsonicAuthenticationAdapter.cs
namespace Melodee.Common.Services.Adapters;

public class OpenSubsonicAuthenticationAdapter
{
    private readonly IAuthenticationService _authenticationService;
    private readonly UserService _userService;
    private readonly ILogger _logger;

    public async Task<ResponseModel> AuthenticateSubsonicApiAsync(
        ApiRequest apiRequest,
        CancellationToken cancellationToken = default)
    {
        // Thin wrapper implementation - delegates to domain services
        // Handles OpenSubsonic-specific response formatting only
    }
}
```

#### Methods to Extract from OpenSubsonicApiService
1. **Line ~850-950**: `AuthenticateSubsonicApiAsync()` method
2. **Line ~1200-1300**: Token validation logic
3. **Line ~200-250**: Rate limiting checks
4. **Line ~300-350**: User permission validation

#### Performance Requirements for Phase 1
- **Database**: Use compiled queries for authentication (target <5ms)
- **Caching**: Cache user permissions for 5 minutes
- **Rate Limiting**: In-memory sliding window, 100 requests/minute per IP
- **Logging**: Async security event logging

#### Testing Requirements for Phase 1
```csharp
// Required test files to create:
// tests/Melodee.Common.Tests/Services/Infrastructure/AuthenticationServiceTests.cs
// tests/Melodee.Common.Tests/Services/UserServiceTests.cs (enhance existing)
// tests/Melodee.Common.Tests/Services/Adapters/OpenSubsonicAuthenticationAdapterTests.cs

// Performance test requirement:
[Fact]
public async Task AuthenticateUser_Under1000ConcurrentRequests_ShouldComplete_Within10ms()
{
    // Load test implementation
}
```

#### Dependency Injection Updates Required
```csharp
// File: /src/Melodee.Common/Extensions/ServiceCollectionExtensions.cs
services.AddScoped<IAuthenticationService, AuthenticationService>();
services.AddScoped<OpenSubsonicAuthenticationAdapter>();
```

#### Success Criteria for Phase 1
- [ ] All authentication logic removed from OpenSubsonicApiService
- [ ] UserService enhanced with compiled queries
- [ ] Authentication performance <10ms P99
- [ ] All tests passing with >90% coverage
- [ ] No breaking changes to existing API contracts

### Goals
- Extract authentication infrastructure from API layer
- Enhance UserService with authorization methods
- Create reusable authentication infrastructure

### New Infrastructure Services
1. **`IAuthenticationService`** (Infrastructure)
   - Token validation and parsing
   - Rate limiting
   - Security logging
   - Protocol-agnostic authentication

### Domain Service Enhancements
1. **`UserService`** enhancements:
   - `ValidateUserCredentialsAsync(username, password)`
   - `ValidateUserTokenAsync(token)`
   - `CheckUserPermissionAsync(userId, permission)`
   - `CanUserShareAsync(userId)`
   - `CanUserAdministerAsync(userId)`

### New API Adapters
1. **`OpenSubsonicAuthenticationAdapter`** (Application)
   - `AuthenticateSubsonicApiAsync()` - thin wrapper
   - OpenSubsonic-specific error formatting
   - API-specific response mapping

### Benefits
- Single source of truth for user authentication in UserService
- Reusable authentication infrastructure
- Clear separation between domain logic and API concerns

---

## Phase 2: Enhance Domain Services for Media and Image Operations
**Files to Create**: 2 infrastructure services
**Files to Enhance**: 3 domain services
**Lines to Refactor**: ~400-500 lines

### Goals
- Move image operations to appropriate domain services
- Extract media streaming infrastructure
- Eliminate direct database access from API layer

### New Infrastructure Services
1. **`IImageProcessingService`** (Infrastructure)
   - Image resizing and format conversion
   - Caching strategies
   - Performance optimization

2. **`IMediaStreamingService`** (Infrastructure)
   - Stream handling and buffering
   - Format conversion
   - Bitrate management

### Domain Service Enhancements
1. **`AlbumService`** enhancements:
   - `GetAlbumCoverArtAsync(albumId, size)`
   - `GetAlbumCoverArtStreamAsync(albumId, size)`
   - Enhanced caching for album images

2. **`ArtistService`** enhancements:
   - `GetArtistImageAsync(artistId, size)`
   - `GetArtistImageStreamAsync(artistId, size)`
   - Artist image management

3. **`UserService`** enhancements:
   - `GetUserAvatarAsync(userId, size)`
   - `SetUserAvatarAsync(userId, imageData)`
   - User avatar management

### API Adapters
1. **`OpenSubsonicMediaAdapter`** (Application)
   - OpenSubsonic-specific media response formatting
   - HTTP streaming concerns
   - Content-type and header management

### Benefits
- Domain services become complete for their domain
- Reusable image/media infrastructure
- Better performance through proper service utilization

---

## Phase 3: Enhance Search and Discovery in Domain Services
**Files to Create**: 1 infrastructure service
**Files to Enhance**: 4 domain services
**Lines to Refactor**: ~300-400 lines

### Goals
- Consolidate search logic in domain services
- Enhance existing search infrastructure
- Improve search performance and consistency

### New Infrastructure Services
1. **`ISearchIndexService`** (Infrastructure)
   - Search index management
   - Query optimization
   - Search result ranking

### Domain Service Enhancements
1. **`ArtistService`** enhancements:
   - `SearchArtistsAsync(query, pagination)`
   - `GetArtistIndexesAsync()`
   - `GetArtistsByLetterAsync(letter)`

2. **`AlbumService`** enhancements:
   - `SearchAlbumsAsync(query, pagination)`
   - `GetAlbumsByGenreAsync(genre)`
   - `GetAlbumsByYearAsync(year)`

3. **`SongService`** enhancements:
   - `SearchSongsAsync(query, pagination)`
   - `GetRandomSongsAsync(criteria)`
   - `GetTopSongsAsync(criteria)`

4. **`LibraryService`** enhancements:
   - `GetMusicDirectoryAsync(id)`
   - `GetLibraryIndexesAsync()`
   - Enhanced browsing capabilities

### API Adapters
1. **`OpenSubsonicSearchAdapter`** (Application)
   - Search result formatting for OpenSubsonic
   - Pagination handling
   - Response structure mapping

### Benefits
- Complete search functionality in domain services
- Consistent search behavior across all interfaces
- Better performance through optimized domain service methods

---

## Phase 4: Enhance Playlist Management in Domain Services
**Files to Enhance**: 2 domain services
**Files to Create**: 1 API adapter
**Lines to Refactor**: ~400-500 lines

### Goals
- Complete playlist functionality in PlaylistService
- Enhance collection operations in domain services
- Remove playlist logic from API layer

### Domain Service Enhancements
1. **`PlaylistService`** enhancements:
   - `GetUserPlaylistsAsync(userId)` - if missing
   - `GetPlaylistWithSongsAsync(playlistId, userId)`
   - `CreatePlaylistAsync(userId, name, description, isPublic)`
   - `UpdatePlaylistAsync(playlistId, userId, updates)`
   - `DeletePlaylistAsync(playlistId, userId)`
   - `AddSongsToPlaylistAsync(playlistId, userId, songIds)`
   - `RemoveSongsFromPlaylistAsync(playlistId, userId, songIds)`

2. **`AlbumService`** enhancements:
   - `GetAlbumCollectionAsync(criteria, pagination)`
   - `GetNewestAlbumsAsync(limit)`
   - `GetRandomAlbumsAsync(limit)`
   - Collection and filtering operations

### API Adapters
1. **`OpenSubsonicPlaylistAdapter`** (Application)
   - Playlist response formatting
   - OpenSubsonic-specific field mapping
   - Error handling and validation

### Benefits
- Complete playlist domain logic in PlaylistService
- Better reusability across different interfaces
- Improved error handling and validation

---

## Phase 5: Enhance Sharing in Domain Services
**Files to Enhance**: 2 domain services
**Files to Create**: 1 API adapter
**Lines to Refactor**: ~200-300 lines

### Goals
- Complete sharing functionality in ShareService and UserService
- Remove sharing logic from API layer
- Better integration with domain models

### Domain Service Enhancements
1. **`ShareService`** enhancements (if missing):
   - `GetUserSharesAsync(userId)`
   - `CreateShareAsync(userId, shareData)`
   - `UpdateShareAsync(shareId, userId, updates)`
   - `DeleteUserSharesAsync(userId, shareIds)`
   - `ValidateShareAccessAsync(shareKey)`

2. **`UserService`** enhancements:
   - `GetUserSharesWithDetailsAsync(userId)` - with related entities
   - Share permission validation methods

### API Adapters
1. **`OpenSubsonicSharingAdapter`** (Application)
   - Share URL generation for OpenSubsonic
   - Response formatting
   - Protocol-specific validation

### Benefits
- Complete sharing domain logic in appropriate services
- Better security through proper domain validation
- Easier to extend sharing features

---

## Phase 6: Enhance User Activity Tracking in Domain Services
**Files to Enhance**: 2 domain services
**Files to Create**: 1 API adapter
**Lines to Refactor**: ~150-200 lines

### Goals
- Complete activity tracking in ScrobbleService and UserService
- Remove activity logic from API layer
- Better analytics and reporting capabilities

### Domain Service Enhancements
1. **`ScrobbleService`** enhancements (if missing):
   - `RecordScrobbleAsync(userId, songId, timestamp)`
   - `GetNowPlayingAsync(userId)`
   - `SetNowPlayingAsync(userId, songId)`
   - `GetUserListeningHistoryAsync(userId, pagination)`

2. **`UserService`** enhancements:
   - `SetSongRatingAsync(userId, songId, rating)`
   - `GetUserSongRatingAsync(userId, songId)`
   - User preference management

### API Adapters
1. **`OpenSubsonicActivityAdapter`** (Application)
   - Activity response formatting
   - Timestamp conversion
   - Protocol-specific data mapping

### Benefits
- Complete user activity domain logic
- Better analytics capabilities
- Consistent activity tracking across interfaces

---

## Phase 7: Enhance System Operations in Domain Services
**Files to Enhance**: 1 domain service
**Files to Create**: 1 infrastructure service, 1 API adapter
**Lines to Refactor**: ~100-150 lines

### Goals
- Complete system operations in LibraryService
- Extract system infrastructure concerns
- Better monitoring and health checking

### New Infrastructure Services
1. **`ISystemHealthService`** (Infrastructure)
   - Health monitoring
   - Performance metrics
   - System status reporting

### Domain Service Enhancements
1. **`LibraryService`** enhancements (if missing):
   - `GetScanStatusAsync()`
   - `StartLibraryScanAsync()`
   - `GetLibraryStatisticsAsync()`

### API Adapters
1. **`OpenSubsonicSystemAdapter`** (Application)
   - System response formatting
   - License information formatting
   - Health check formatting

### Benefits
- Complete system domain logic
- Better monitoring capabilities
- Consistent system operations

---

## Phase 8: Transform OpenSubsonicApiService into Thin Coordinator
**Files to Modify**: 1 existing file
**Lines to Refactor**: Remaining ~1000 lines

### Goals
- Remove ALL business logic from OpenSubsonicApiService
- Transform into pure API coordinator
- Implement proper error handling and logging

### New Responsibilities (API Layer Only)
1. **Request Parsing**: Parse OpenSubsonic API requests
2. **Authentication Coordination**: Delegate to AuthenticationService
3. **Service Orchestration**: Coordinate calls to domain services and adapters
4. **Response Formatting**: Format final OpenSubsonic responses
5. **Error Handling**: Handle and format API-specific errors
6. **Logging**: Request/response logging

### Architecture After Refactoring
```
OpenSubsonicApiService (Thin Coordinator)
├── IAuthenticationService (Infrastructure)
├── API Adapters (Application Layer)
│   ├── OpenSubsonicAuthenticationAdapter
│   ├── OpenSubsonicMediaAdapter
│   ├── OpenSubsonicSearchAdapter
│   ├── OpenSubsonicPlaylistAdapter
│   ├── OpenSubsonicSharingAdapter
│   ├── OpenSubsonicActivityAdapter
│   └── OpenSubsonicSystemAdapter
├── Domain Services (Domain Layer)
│   ├── UserService (Enhanced)
│   ├── AlbumService (Enhanced)
│   ├── ArtistService (Enhanced)
│   ├── SongService (Enhanced)
│   ├── PlaylistService (Enhanced)
│   ├── ShareService (Enhanced)
│   ├── ScrobbleService (Enhanced)
│   └── LibraryService (Enhanced)
└── Infrastructure Services
    ├── IImageProcessingService
    ├── IMediaStreamingService
    ├── ISearchIndexService
    └── ISystemHealthService
```

### Benefits
- True separation of concerns following DDD
- Highly testable and maintainable architecture
- Reusable domain services across different interfaces
- Clear dependency flow and responsibilities

---

## Implementation Guidelines

### Domain Service Enhancement Rules
1. **Add methods to existing services** instead of creating new services
2. **Keep domain services technology-agnostic**
3. **Ensure domain services are complete** for their domain
4. **No cross-domain dependencies** between domain services

### API Adapter Patterns
1. **Thin wrappers** around domain service calls
2. **Protocol-specific formatting** only
3. **No business logic** in adapters
4. **Consistent error handling** patterns

### Infrastructure Service Guidelines
1. **Support multiple domain services** when appropriate
2. **Technology-specific implementations**
3. **Configurable and testable**
4. **Clear interface contracts**

### Critical Performance Considerations for High-Throughput Service

#### Performance Requirements Analysis
Given the service handles thousands of requests per second, the refactoring must address:
- **Latency**: Sub-100ms response times for most operations
- **Throughput**: Support for 5000+ concurrent requests
- **Memory**: Efficient memory usage to prevent GC pressure
- **Database**: Optimized queries and connection pooling
- **Caching**: Multi-layer caching strategy
- **Scalability**: Horizontal scaling capabilities

#### Performance-Critical Areas Missed in Original Plan

##### 1. Database Performance Optimization
**Critical Issue**: Current service uses inefficient EF queries and lacks proper indexing strategy

**Enhanced Approach:**
- **Connection Pooling**: Optimize DbContext factory configuration
- **Query Optimization**: Replace LINQ with raw SQL for hot paths
- **Database Indexes**: Ensure proper indexing on frequently queried fields
- **Read Replicas**: Separate read/write operations for scaling

```csharp
// Example: High-performance user authentication
public class UserService
{
    // Hot path - use compiled query for performance
    private static readonly Func<MelodeeDbContext, string, string, Task<User?>>
        AuthenticateUserQuery = EF.CompileAsyncQuery(
            (MelodeeDbContext context, string username, string passwordHash) =>
                context.Users
                    .Where(u => u.Username == username && u.PasswordHash == passwordHash)
                    .FirstOrDefault());

    public async Task<AuthResult> ValidateUserCredentialsAsync(string username, string password)
    {
        // Use compiled query for sub-10ms authentication
        var user = await AuthenticateUserQuery(context, username, ComputeHash(password));
        return new AuthResult { IsSuccess = user != null, User = user };
    }
}
```

##### 2. Multi-Layer Caching Strategy
**Critical Issue**: Insufficient caching leads to repeated database hits

**Enhanced Caching Architecture:**
```
L1 Cache (In-Memory) -> L2 Cache (Redis) -> Database
```

**Implementation:**
```csharp
public interface IPerformanceCacheService : IDisposable
{
    // Hot data - keep in memory for microsecond access
    Task<T?> GetHotAsync<T>(string key) where T : class;
    Task SetHotAsync<T>(string key, T value, TimeSpan expiry) where T : class;

    // Warm data - Redis cache for millisecond access
    Task<T?> GetWarmAsync<T>(string key) where T : class;
    Task SetWarmAsync<T>(string key, T value, TimeSpan expiry) where T : class;

    // Batch operations for efficiency
    Task<Dictionary<string, T?>> GetBatchAsync<T>(IEnumerable<string> keys) where T : class;
    Task SetBatchAsync<T>(Dictionary<string, T> items, TimeSpan expiry) where T : class;
}

// Usage in domain services
public class AlbumService
{
    public async Task<Album?> GetAlbumAsync(Guid albumId)
    {
        // Try L1 cache first (microseconds)
        var album = await _performanceCache.GetHotAsync<Album>($"album:{albumId}");
        if (album != null) return album;

        // Try L2 cache (milliseconds)
        album = await _performanceCache.GetWarmAsync<Album>($"album:{albumId}");
        if (album != null)
        {
            // Promote to L1 cache
            await _performanceCache.SetHotAsync($"album:{albumId}", album, TimeSpan.FromMinutes(5));
            return album;
        }

        // Database hit only if not cached
        album = await GetAlbumFromDatabaseAsync(albumId);
        if (album != null)
        {
            // Cache in both layers
            await _performanceCache.SetHotAsync($"album:{albumId}", album, TimeSpan.FromMinutes(5));
            await _performanceCache.SetWarmAsync($"album:{albumId}", album, TimeSpan.FromHours(1));
        }

        return album;
    }
}
```

##### 3. Memory and GC Optimization
**Critical Issue**: Object allocation pressure causes GC pauses under high load

**Memory Optimization Strategy:**
```csharp
// Use object pooling for frequently created objects
public interface IObjectPoolService
{
    ObjectPool<T> GetPool<T>() where T : class, new();
}

// Use ArrayPool for byte arrays (images, audio streams)
public class ImageProcessingService
{
    private readonly ArrayPool<byte> _byteArrayPool = ArrayPool<byte>.Shared;

    public async Task<byte[]> ProcessImageAsync(byte[] input, int targetSize)
    {
        var buffer = _byteArrayPool.Rent(input.Length * 2); // Rent from pool
        try
        {
            // Process image using rented buffer
            var result = ProcessImageInternal(input, buffer, targetSize);
            return result;
        }
        finally
        {
            _byteArrayPool.Return(buffer); // Return to pool
        }
    }
}

// Use Span<T> and Memory<T> for zero-allocation string operations
public static class ApiIdHelper
{
    public static Guid? ExtractApiKey(ReadOnlySpan<char> id)
    {
        // Zero-allocation string parsing
        var separatorIndex = id.IndexOf(':');
        if (separatorIndex == -1) return null;

        var guidSpan = id.Slice(separatorIndex + 1);
        return Guid.TryParse(guidSpan, out var guid) ? guid : null;
    }
}
```

##### 4. Asynchronous Processing and Batching
**Critical Issue**: Synchronous operations and N+1 queries kill performance

**Batch Processing Strategy:**
```csharp
public class PlaylistService
{
    // Batch load songs to avoid N+1 queries
    public async Task<Playlist> GetPlaylistWithSongsAsync(Guid playlistId, Guid userId)
    {
        var playlist = await GetPlaylistAsync(playlistId);
        if (playlist == null) return null;

        // Single query to load all songs
        var songIds = playlist.SongIds; // Assume we store this efficiently
        var songs = await _songService.GetSongsBatchAsync(songIds);
        var userSongs = await _userService.GetUserSongsBatchAsync(userId, songIds);

        // Efficient in-memory join
        playlist.Songs = songs.Join(userSongs,
            s => s.Id,
            us => us.SongId,
            (song, userSong) => song.WithUserData(userSong))
            .ToList();

        return playlist;
    }
}
```

##### 5. Response Streaming and Compression
**Critical Issue**: Large responses consume memory and bandwidth

**Streaming Strategy:**
```csharp
public interface IStreamingResponseService
{
    IAsyncEnumerable<T> StreamResultsAsync<T>(IQueryable<T> query);
    Task WriteJsonStreamAsync<T>(Stream output, IAsyncEnumerable<T> items);
}

// Usage for large result sets
public class OpenSubsonicSearchAdapter
{
    public async Task StreamSearchResultsAsync(SearchRequest request, Stream responseStream)
    {
        var query = BuildSearchQuery(request);
        var results = _streamingService.StreamResultsAsync(query);

        await _streamingService.WriteJsonStreamAsync(responseStream, results);
    }
}
```

##### 6. Circuit Breaker and Rate Limiting
**Critical Issue**: Cascading failures under high load

**Resilience Strategy:**
```csharp
public interface IResilienceService
{
    Task<T> ExecuteWithCircuitBreakerAsync<T>(Func<Task<T>> operation, string circuitName);
    Task<bool> CheckRateLimitAsync(string key, int maxRequests, TimeSpan window);
}

public class OpenSubsonicAuthenticationAdapter
{
    public async Task<AuthResult> AuthenticateAsync(ApiRequest request)
    {
        // Rate limiting per IP
        var canProceed = await _resilienceService.CheckRateLimitAsync(
            $"auth:{request.ClientIP}", 100, TimeSpan.FromMinutes(1));

        if (!canProceed)
            return AuthResult.RateLimited();

        // Circuit breaker for database calls
        return await _resilienceService.ExecuteWithCircuitBreakerAsync(
            () => _userService.ValidateCredentialsAsync(request.Username, request.Password),
            "user-auth");
    }
}
```

#### Performance Testing and Monitoring Strategy

##### Load Testing Framework
```csharp
// Performance test example
[Fact]
public async Task UserAuthentication_UnderLoad_ShouldMaintainPerformance()
{
    var tasks = new List<Task<TimeSpan>>();

    // Simulate 1000 concurrent authentication requests
    for (int i = 0; i < 1000; i++)
    {
        tasks.Add(MeasureAuthenticationTime($"user{i}", "password{i}"));
    }

    var results = await Task.WhenAll(tasks);

    // Assert performance requirements
    results.Average().Should().BeLessThan(TimeSpan.FromMilliseconds(50)); // P50 < 50ms
    results.Max().Should().BeLessThan(TimeSpan.FromMilliseconds(200));    // P99 < 200ms
}
```

##### Performance Monitoring
```csharp
public class PerformanceMetricsService
{
    private readonly IMetricsLogger _metrics;

    public async Task<T> MeasureAsync<T>(string operationName, Func<Task<T>> operation)
    {
        using var timer = _metrics.StartTimer($"{operationName}.duration");
        try
        {
            var result = await operation();
            _metrics.Counter($"{operationName}.success").Increment();
            return result;
        }
        catch (Exception ex)
        {
            _metrics.Counter($"{operationName}.error").Increment();
            _metrics.Counter($"{operationName}.error.{ex.GetType().Name}").Increment();
            throw;
        }
    }
}
```

#### Database Performance Optimization

##### Query Optimization Rules
1. **Use compiled queries for hot paths** (authentication, common lookups)
2. **Implement proper indexing strategy** on all foreign keys and search fields
3. **Use raw SQL for complex queries** instead of LINQ translations
4. **Implement database result streaming** for large datasets
5. **Use read replicas** for read-heavy operations

##### Database Schema Optimization
```sql
-- High-performance indexes for hot queries
CREATE INDEX IX_Users_Username_PasswordHash ON Users(Username, PasswordHash) INCLUDE (Id, IsActive);
CREATE INDEX IX_Songs_AlbumId_TrackNumber ON Songs(AlbumId, TrackNumber) INCLUDE (Id, Title, Duration);
CREATE INDEX IX_UserSongs_UserId_SongId ON UserSongs(UserId, SongId) INCLUDE (Rating, PlayCount, LastPlayed);

-- Partitioning for large tables
CREATE PARTITION FUNCTION PF_CreatedDate (datetime2)
AS RANGE RIGHT FOR VALUES ('2024-01-01', '2024-07-01', '2025-01-01');
```

#### Revised Phase Implementation with Performance Focus

Each phase now includes mandatory performance requirements:

**Phase 1 Enhanced**: Authentication + Performance Infrastructure
- Implement compiled queries for authentication
- Add connection pooling optimization
- Implement rate limiting and circuit breakers
- **Performance Target**: <10ms authentication time

**Phase 2 Enhanced**: Media/Images + Caching Infrastructure
- Implement multi-layer caching
- Add object pooling for image processing
- Implement streaming for large media files
- **Performance Target**: <50ms for cached images, <200ms for processed images

**Phase 3-8 Enhanced**: All phases now include:
- Performance benchmarking requirements
- Memory optimization strategies
- Database query optimization
- Caching implementation
- Load testing requirements

This performance-focused approach ensures the refactoring not only improves code organization but also maintains and improves the high-throughput requirements of the service.
