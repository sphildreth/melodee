using Melodee.Common.Enums;

namespace Melodee.Common.Metadata.AudioTags.Models;

public readonly record struct AudioImage
{
    public ReadOnlyMemory<byte> Data { get; init; }
    public string MimeType { get; init; }
    public string? Description { get; init; }
    public PictureIdentifier Type { get; init; }
}
