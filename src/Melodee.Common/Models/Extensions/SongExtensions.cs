using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models.Extensions;

public static class SongExtensions
{
    private static readonly Regex UnwantedSongTitleTextRegex = new(@"(\s{2,}|(\s\(prod\s))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static T? MetaTagValue<T>(this Song song, MetaTagIdentifier metaTagIdentifier)
    {
        var d = default(T?);
        if (song.Tags == null || !song.Tags.Any())
        {
            return d;
        }

        try
        {
            var vv = song.Tags?.FirstOrDefault(x => x.Identifier == metaTagIdentifier)?.Value;
            if (vv == null)
            {
                return d;
            }

            if (vv is JsonElement)
            {
                vv = vv.ToString() ?? string.Empty;
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

    public static T? MediaAudioValue<T>(this Song song, MediaAudioIdentifier mediaAudioIdentifier)
    {
        var d = default(T?);
        if (song.MediaAudios == null || !song.MediaAudios.Any())
        {
            return d;
        }

        try
        {
            var vv = song.MediaAudios?.FirstOrDefault(x => x.Identifier == mediaAudioIdentifier)?.Value;
            if (vv == null)
            {
                return d;
            }

            if (vv is JsonElement)
            {
                vv = vv.ToString() ?? string.Empty;
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

    public static string? SongArtist(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.Artist);
    }
    
    public static long? SongArtistUniqueId(this Song song)
    {
        var songArtist = song.SongArtist().Nullify();
        if (songArtist == null)
        {
            return null;
        }
        return SafeParser.Hash(songArtist);
    }    

    public static string? AlbumArtist(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.AlbumArtist);
    }

    public static string? AlbumTitle(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.Album);
    }

   
    public static int? AlbumYear(this Song song)
    {
        return song.MetaTagValue<int?>(MetaTagIdentifier.OrigAlbumYear) ??
               song.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
               song.MetaTagValue<int?>(MetaTagIdentifier.OrigAlbumDate) ??
               song.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear);
    }

    public static int? SongYear(this Song song)
    {
        return song.MetaTagValue<int?>(MetaTagIdentifier.OrigAlbumYear) ??
               song.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
               song.MetaTagValue<int?>(MetaTagIdentifier.OrigAlbumDate) ??
               song.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear);
    }

    public static string? AlbumDate(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.Date) ??
               song.MetaTagValue<string?>(MetaTagIdentifier.OrigAlbumDate) ??
               song.MetaTagValue<string?>(MetaTagIdentifier.RecordingDateOrYear);
    }

    public static double? Duration(this Song song)
    {
        return song.MediaAudioValue<double?>(MediaAudioIdentifier.DurationMs);
    }

    public static string? DurationTime(this Song song)
    {
        return song.Duration().HasValue ? new TimeInfo(SafeParser.ToNumber<decimal>(song.Duration()!.Value)).ToFullFormattedString() : "--:--";
    }

    public static string? DurationTimeShort(this Song song)
    {
        return song.Duration().HasValue ? new TimeInfo(SafeParser.ToNumber<decimal>(song.Duration()!.Value)).ToShortFormattedString() : "--:--";
    }

    public static string? Title(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.Title);
    }
    
    public static string? SubTitle(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.SubTitle);
    }    

    public static DateTime? AlbumDateValue(this Song song)
    {
        return song.AlbumDate().Nullify() == null ? null : DateTime.Parse(song.AlbumDate()!);
    }

    public static int SongNumber(this Song song)
    {
        return song.MetaTagValue<int?>(MetaTagIdentifier.TrackNumber) ?? 0;
    }

    public static int SongTotalNumber(this Song song)
    {
        return song.MetaTagValue<int?>(MetaTagIdentifier.SongTotal) ?? 0;
    }

    public static int MediaNumber(this Song song)
    {
        return song.MetaTagValue<int?>(MetaTagIdentifier.DiscNumber) ?? 0;
    }
    
    public static string? MediaSubTitle(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.SubTitle);
    }    

    public static string? Genre(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.Genre);
    }

    public static int MediaTotalNumber(this Song song)
    {
        return song.MetaTagValue<int?>(MetaTagIdentifier.DiscTotal) ??
               song.MetaTagValue<int?>(MetaTagIdentifier.DiscNumberTotal) ?? 0;
    }

    public static bool IsValid(this Song song, Dictionary<string, object?> configuration)
    {
        if (song.Tags?.Count() == 0)
        {
            return false;
        }

        // Only test for AlbumUniqueId validity if the Album Title tag is present.
        long albumUniqueId = 1;
        var albumTitle = song.AlbumTitle().Nullify();
        if (!string.IsNullOrWhiteSpace(albumTitle))
        {
            albumUniqueId = song.AlbumUniqueId;
        }

        var songNumber = song.SongNumber();
        var mediaNumber = song.MediaNumber();
        return song is { SongId: > 0, UniqueId: > 0 } &&
               albumUniqueId > 0 &&
               songNumber > 0 &&
               songNumber < SafeParser.ToNumber<int>(configuration[SettingRegistry.ValidationMaximumSongNumber]) &&
               mediaNumber < SafeParser.ToNumber<int>(configuration[SettingRegistry.ValidationMaximumMediaNumber]) &&
               song.Title().Nullify() != null;
    }

    public static bool TitleHasUnwantedText(this Song song)
    {
        var albumTitle = song.AlbumTitle() ?? string.Empty;
        var songTitle = song.Title();
        if (string.IsNullOrWhiteSpace(songTitle))
        {
            return true;
        }

        if (songTitle.HasFeaturingFragments())
        {
            return true;
        }

        try
        {
            if (UnwantedSongTitleTextRegex.IsMatch(songTitle))
            {
                return true;
            }

            if (songTitle.Any(char.IsDigit))
            {
                var songNumber = song.SongNumber();
                if (string.Equals(songTitle.Trim(), songNumber.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return Regex.IsMatch(songTitle, $@"^({Regex.Escape(albumTitle)}\s*.*\s*)?([0-9]*{songNumber}\s)");
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"TitleHasUnwantedText For AlbumTitle [{albumTitle}] for SongTitle [{songTitle}] Error [{ex.Message}] ", "Error");
        }

        return false;
    }

    public static string SongFileName(int songNumber, string? songTitle, int mediaNumber)
    {
        if (songNumber < 1)
        {
            throw new Exception($"Invalid Song number [{songNumber}]");
        }

        if (string.IsNullOrWhiteSpace(songTitle))
        {
            throw new Exception($"Invalid Song title [{songTitle}]");
        }

        var songNumberValue = songNumber.ToString("D3");

        // Put an "m" for media on the TPOS greater than 1 so the directory sorts proper
        /*
          Example when not part of a media set Song 7
          "007 Something.mpg"

          Example media 1 Song 14
          "001-014 Something.mpg"

          Example media 2 Song 5
          "002-005 Something Else.mpg"
        */
        var disc = mediaNumber > 1 ? $"{mediaNumber:D3}-" : string.Empty;

        // Get new name for file
        var fileNameFromTitle = songTitle.ToTitleCase(false)?.ToFileNameFriendly();
        if (fileNameFromTitle != null && fileNameFromTitle.StartsWith(songNumberValue))
        {
            fileNameFromTitle = fileNameFromTitle
                .RemoveStartsWith($"{songNumberValue} -")
                .RemoveStartsWith($"{songNumberValue} ")
                .RemoveStartsWith($"{songNumberValue}.")
                .RemoveStartsWith($"{songNumberValue}-")
                .ToTitleCase(false);
        }

        return $"{disc}{songNumberValue} {fileNameFromTitle}.mp3";
    }

    public static string ToSongFileName(this Song song)
    {
        return SongFileName(song.SongNumber(), song.Title(), song.MediaNumber());
    }
}
