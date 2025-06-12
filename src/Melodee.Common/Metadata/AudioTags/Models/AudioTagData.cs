using Melodee.Common.Enums;

namespace Melodee.Common.Metadata.AudioTags.Models;

public record struct AudioTagData
{
    public AudioFormat Format { get; init; }
    public Dictionary<MetaTagIdentifier, object> Tags { get; init; }
    public AudioFileMetadata FileMetadata { get; init; }
    public IReadOnlyList<AudioImage> Images { get; init; }
}
