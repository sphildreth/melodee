# Melodee TODO List

> **Last Updated:** July 8, 2025  
> **Items ranked by user impact (highest impact first)**

## 🔴 High Impact - Core User Experience

### 1. **OpenSubsonic API Implementation** 
- **Location:** `src/Melodee.Blazor/Controllers/OpenSubsonic/`
- **Impact:** Critical for client compatibility (music players, mobile apps)
- **Items:**
  - [ ] Complete UserManagementController implementation
    - [ ] `getUsers` endpoint
    - [ ] `updateUser` endpoint  
    - [ ] `deleteUser` endpoint
    - [ ] `changePassword` endpoint
  - [ ] Complete JukeboxController implementation
    - [ ] `jukeboxControl` endpoint for remote playback control
  - [ ] Complete BrowsingController implementation

### 2. **Audio Tag Management (ID3v2)**
- **Location:** `src/Melodee.Common/Metadata/AudioTags/Writers/Id3v2TagWriter.cs`
- **Impact:** Essential for music metadata editing and organization
- **Items:**
  - [ ] Implement writing/updating multiple ID3v2.4 tags
  - [ ] Implement removing single ID3v2.4 tags
  - [ ] Implement adding images to ID3v2.4 tags
  - [ ] Implement removing all images from ID3v2.4 tags

### 3. **Media Content Type Detection**
- **Location:** `src/Melodee.Common/Data/Models/Extensions/SongExtensions.cs`
- **Impact:** Affects media streaming compatibility and transcoding
- **Items:**
  - [ ] Fix hardcoded "audio/mpeg" type - should detect actual file format (FLAC, MP3, etc.)
  - [ ] Implement proper media type detection for various audio formats

### 4. **Streaming & Transcoding**
- **Location:** `src/Melodee.Common/Services/OpenSubsonicApiService.cs`
- **Impact:** Critical for audio playback functionality
- **Items:**
  - [ ] Implement streaming offset support (start at specific time)
  - [ ] Implement transcoding for format and maxBitRate requirements
  - [ ] Optimize image clearing in regions (currently inefficient)

## 🟡 Medium Impact - User Features & Performance

### 5. **Scrobbling Integration**
- **Location:** `src/Melodee.Common/Plugins/Scrobbling/LastFmScrobbler.cs`
- **Impact:** Important for users who track listening habits
- **Items:**
  - [ ] Implement user.LastFmSessionKey support or new session key retrieval

### 6. **Caching System Enhancement**
- **Location:** `src/Melodee.Common/Services/Caching/MemoryCacheManager.cs`
- **Impact:** Performance optimization for better user experience
- **Items:**
  - [ ] Implement region-based caching with ConcurrentDictionary of MemoryCache

### 7. **Pagination Support**
- **Location:** Multiple locations
- **Impact:** Better performance and UX for large libraries
- **Items:**
  - [ ] Add pagination to playlist queries (`OpenSubsonicApiService.cs:750`)
  - [ ] Add pagination to users list (`UsersController.cs:133`)

### 8. **Multi-Library Support**
- **Location:** Blazor UI components
- **Impact:** Enhanced organization for users with multiple music sources
- **Items:**
  - [ ] Implement library selection prompts when moving media (`ArtistEdit.razor:404`)
  - [ ] Add library selection for album management (`AlbumDetail.razor:360`)
  - [ ] Add library selection for media library operations (`Library.razor:370`)

## 🟢 Low Impact - Technical Improvements

### 9. **API Reliability**
- **Location:** `src/Melodee.Common/Plugins/SearchEngine/Spotify/Spotify.cs`
- **Impact:** Improved stability for metadata searches
- **Items:**
  - [ ] Implement Polly for resilient HTTP calls with timeouts and retries

### 10. **Data Model Completeness**
- **Location:** `src/Melodee.Common/Data/Models/Extensions/`
- **Impact:** API completeness and data integrity
- **Items:**
  - [ ] Complete missing OpenSubsonic response fields (multiple TODO items in AlbumExtensions and SongExtensions)
  - [ ] Implement similar artists functionality (`OpenSubsonicApiService.cs:3347`)
  - [ ] Add support for Navidrome-specific features (`OpenSubsonicApiService.cs:1814`)

### 11. **Event Handling**
- **Location:** `src/Melodee.Common/MessageBus/EventHandlers/AlbumUpdatedEventHandler.cs`
- **Impact:** System consistency and data synchronization
- **Items:**
  - [ ] Implement database updates or other actions for album update events

### 12. **Data Validation**
- **Location:** `src/Melodee.Common/Services/ServiceBase.cs`
- **Impact:** Data integrity and debugging
- **Items:**
  - [ ] Investigate and fix song.CrcHash calculation issues

---

## 📊 Summary

- **Total Items:** 25+ TODO items identified
- **High Impact:** 12 items (Core API, metadata, streaming)
- **Medium Impact:** 8 items (Features, performance, UX)
- **Low Impact:** 5+ items (Technical debt, completeness)

## 🎯 Recommended Priority Order

1. **Start with OpenSubsonic API completion** - Critical for third-party client support
2. **Complete ID3v2 tag management** - Essential for music library management
3. **Fix media type detection** - Important for proper streaming
4. **Implement streaming features** - Core playback functionality
5. **Add pagination support** - Performance for large libraries
6. **Enhance caching system** - Overall performance improvement

> **Note:** This analysis is based on code comments marked with "TODO" found throughout the codebase. Regular updates to this list should be made as items are completed or new ones are identified.