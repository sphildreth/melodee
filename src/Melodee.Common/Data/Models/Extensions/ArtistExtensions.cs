using Melodee.Common.Data.Contants;
using Melodee.Common.Extensions;

namespace Melodee.Common.Data.Models.Extensions;

public static class ArtistExtensions
{
    public static string ToCoverArtId(this Artist artist) => artist.ToApiKey();
    
    public static string ToApiKey(this Artist artist) => $"artist{OpenSubsonicServer.ApiIdSeparator}{artist.ApiKey}";

    public static Common.Models.OpenSubsonic.ArtistID3 ToApiArtistID3(this Artist artist, UserArtist? userArtist = null)
    {
        return new Common.Models.OpenSubsonic.ArtistID3(
            artist.ToApiKey(),
            artist.Name,
            artist.ToCoverArtId(),
            artist.AlbumCount,
            userArtist?.Rating ?? 0,
            "URL",
            userArtist?.StarredAt.ToString(),
            artist.MusicBrainzId?.ToString(),
            artist.SortName,
            artist.Roles?.ToTags()?.ToArray()
        );
    }    
    
    public static Common.Models.OpenSubsonic.Artist ToApiArtist(this Artist artist, UserArtist? userArtist = null)
    {
        return new Common.Models.OpenSubsonic.Artist(
            artist.ToApiKey(),
            artist.Name,
            artist.AlbumCount,
            userArtist?.Rating ?? 0,
            artist.CalculatedRating,
            artist.ToCoverArtId(),
            "Url", // TODO ?
            userArtist?.StarredAt.ToString()
            );
        
    }
}
