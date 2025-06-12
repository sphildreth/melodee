# Melodee Audio Tag Library

This library provides async, thread-safe reading and writing of audio metadata tags for multiple formats, with no
external dependencies.

## Features

- Fast, signature-based file format detection (no reliance on file extensions)
- Read and write support for:
    - ID3v1, ID3v2.3, ID3v2.4 (MP3)
    - APEv2
    - iTunes MP4
    - WMA
    - Vorbis
- Read and write images (cover art, etc.) where supported
- All tag data accessible via a dictionary keyed by `MetaTagIdentifier`
- File metadata (size, modification date, etc.) included
- Fully async API with cancellation support
- No caching, no external libraries
- Full xUnit test coverage

## Usage Example

```csharp
using Melodee.Common.Metadata.AudioTags;
using System.Threading;

// Read all tags and images from a file
var tagData = await AudioTagManager.ReadAllTagsAsync("song.mp3", CancellationToken.None);

// Access tags
var title = tagData.Tags[MetaTagIdentifier.Title];
var artist = tagData.Tags[MetaTagIdentifier.Artist];

// Access images
foreach (var image in tagData.Images)
{
    // Use image.Data, image.MimeType, etc.
}

// Access file metadata
var size = tagData.FileMetadata.FileSize;
var modified = tagData.FileMetadata.LastModified;
```

## Writing Tags

```csharp
using Melodee.Common.Metadata.AudioTags.Writers;

var writer = new Id3v2TagWriter();
await writer.WriteTagAsync("song.mp3", MetaTagIdentifier.Title, "New Title");
```

## Testing

- All methods are covered by xUnit tests in `tests/Melodee.Tests/MetaData/AudioTags`.

## Thread Safety

- All operations are async and thread-safe.

## License

MIT
