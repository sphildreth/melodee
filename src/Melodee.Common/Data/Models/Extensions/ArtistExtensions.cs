using Melodee.Common.Data.Contants;

namespace Melodee.Common.Data.Models.Extensions;

public static class ArtistExtensions
{
    public static string ToCoverArtId(this Artist artist) => artist.ToApiKey();
    
    public static string ToApiKey(this Artist artist) => $"artist{OpenSubsonicServer.ApiIdSeparator}{artist.ApiKey}";
}
