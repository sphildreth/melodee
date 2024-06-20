using System.ComponentModel;
using System.Diagnostics;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;

namespace Melodee.Common.Models.Extensions;

public static class ReleaseExtensions
{
    public static T? MetaTagValue<T>(this Release release, MetaTagIdentifier metaTagIdentifier)
    {
        var d = default(T?);
        if (release.Tags == null || !release.Tags.Any())
        {
            return d;
        }
        try
        {
            var vv = release.Tags?.FirstOrDefault(x => x.Identifier == metaTagIdentifier)?.Value;
            if (vv == null)
            {
                return d;
            }
            var converter = TypeDescriptor.GetConverter(typeof(T?));
            return (T?)converter.ConvertFrom(vv);           
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
        }
        return d;
    }

    public static ReleaseArtistType ArtistType(this Release release)
    {
        var artist = release.Artist();
        if (string.IsNullOrWhiteSpace(artist))
        {
            return ReleaseArtistType.NotSet;
        }
        if (artist.IsVariousArtistValue())
        {
            return ReleaseArtistType.VariousArtists;
        }
        if (release.Genre() == Genres.Soundtrack.ToString() || artist.IsSoundTrackAristValue())
        {
            return ReleaseArtistType.SoundTrack;
        }        
        if (release.Tracks != null)
        {
            if (release.Tracks.Any(x => string.Equals(x.TrackArtist(), artist, StringComparison.OrdinalIgnoreCase)))
            {
                return ReleaseArtistType.VariousArtists;
            }
            if (release.Tracks.Any(x => !string.Equals(x.TrackArtist(), artist, StringComparison.OrdinalIgnoreCase)))
            {
                return ReleaseArtistType.VariousArtists;
            }
        }
        return ReleaseArtistType.ArtistOrBand;
    }
    
    
    public static string? Artist(this Release release) => release.MetaTagValue<string?>(MetaTagIdentifier.Artist);
    
    public static string? ReleaseTitle(this Release release) => release.MetaTagValue<string?>(MetaTagIdentifier.Album);
    
    public static int? ReleaseYear(this Release release) => release.MetaTagValue<int?>(MetaTagIdentifier.OrigReleaseYear) ?? 
                                                            release.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
                                                            release.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear);
    public static int MediaCountValue(this Release release) => release.MetaTagValue<int?>(MetaTagIdentifier.DiscNumberTotal) ?? 
                                                               release.MetaTagValue<int?>(MetaTagIdentifier.DiscTotal) ?? 
                                                               0;

    public static int TrackCountValue(this Release release) => release.MetaTagValue<int?>(MetaTagIdentifier.TrackTotal) ?? 0;

    public static string? Genre(this Release release) => release.MetaTagValue<string?>(MetaTagIdentifier.Genre);
}