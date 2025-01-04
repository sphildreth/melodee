using Melodee.Common.Extensions;
using NodaTime;

namespace Melodee.Common.Models.Cards;

public sealed record ArtistCard
{
    public bool IsValid { get; init; }

    public bool IsSelected { get; set; }

    public object? State { get; set; }

    public required Guid Id { get; init; }
    
    public required string ApiKeyId { get; init; }
    
    public required string Name { get; init; }
    
    public int AlbumCount { get; init; }
    
    public int SongCount { get; init; }
    
    public byte[]? ImageBytes { get; init; }

    public string? ImageBase64 => ImageBytes == null ? null : $"data:image/jpeg;base64,{Convert.ToBase64String(ImageBytes)}";

    public required Instant Created { get; init; }
    
    public string InfoLineTitle => $"Album Count | Song Count";

    public string InfoLineData => $"{AlbumCount.ToStringPadLeft(4)} | {SongCount.ToStringPadLeft(5)}";    
}
