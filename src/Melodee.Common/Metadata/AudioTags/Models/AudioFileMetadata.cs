namespace Melodee.Common.Metadata.AudioTags.Models;

public readonly record struct AudioFileMetadata
{
    public long FileSize { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public DateTimeOffset Created { get; init; }
    public string FilePath { get; init; }
}
