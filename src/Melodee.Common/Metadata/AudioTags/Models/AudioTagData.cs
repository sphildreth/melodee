using Melodee.Common.Enums;

namespace Melodee.Common.Metadata.AudioTags.Models;

public record struct AudioTagData
{
    public AudioFormat Format { get; init; }
    public Dictionary<MetaTagIdentifier, object> Tags { get; init; }
    public AudioFileMetadata FileMetadata { get; init; }
    public IReadOnlyList<AudioImage> Images { get; init; }

    public bool IsValid()
    {
        // Ensure that the tags dictionary is not null, contains the required tags from AudioTagManager.DefaultTags, and that the file metadata is valid
        if (Tags == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(FileMetadata.FilePath) || FileMetadata.FileSize <= 0)
        {
            return false;
        }

        foreach (var tag in AudioTagManager.DefaultTags)
        {
            if (!Tags.ContainsKey(tag.Key) || (tag.Value is string str && string.IsNullOrWhiteSpace(str)) || tag.Value is int and <= 0)
            {
                return false;
            }
        }

        return true;
    }
}
