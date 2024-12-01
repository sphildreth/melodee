using Melodee.Common.Data.Contants;

namespace Melodee.Common.Data.Models.Extensions;

public static class ArtistExtensions
{
    public static string ToCoverArtId(this Artist artist) => artist.ToApiKey();
    
    public static string ToApiKey(this Artist artist) => $"artist{OpenSubsonicServer.ApiIdSeparator}{artist.ApiKey}";

    public static Common.Models.OpenSubsonic.Artist ToApiArtist(this Artist artist, UserArtist? userArtist = null)
    {
        return new Common.Models.OpenSubsonic.Artist(
            artist.ToApiKey(),
            artist.Name,
            artist.AlbumCount,
            0,
            artist.CalculatedRating,
            artist.ToCoverArtId(),
            "Url", // TODO ?
            userArtist?.StarredAt.ToString()
            );
        
    }
}
