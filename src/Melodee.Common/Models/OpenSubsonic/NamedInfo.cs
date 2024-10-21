namespace Melodee.Common.Models.OpenSubsonic;

public record NamedInfo(
    string Id,
    string Name,    
    string? SmallImageUrl,
    string? MediumImageUrl,
    string? LargeImageUrl,
    int? SongCount,
    int? AlbumCount) : InfoBase(SmallImageUrl, MediumImageUrl, LargeImageUrl);
 
