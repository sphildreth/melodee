namespace Melodee.Common.Models;

public sealed record DefaultImages()
{
    public required byte[] UserAvatarBytes { get; init; }

    public required byte[] AlbumCoverBytes { get; init; }
    
    public required byte[] ArtistBytes { get; init; }
    public string UserAvatarBase64 => $"data:image/png;base64,{Convert.ToBase64String(UserAvatarBytes)}";
    
    public string AlbumCoverBase64 => $"data:image/png;base64,{Convert.ToBase64String(AlbumCoverBytes)}";
    
    public string ArtistBase64 => $"data:image/png;base64,{Convert.ToBase64String(ArtistBytes)}";
}
