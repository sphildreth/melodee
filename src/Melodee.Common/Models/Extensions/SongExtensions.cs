using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using NodaTime;
using ServiceStack;

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

        var tType = typeof(T?);
        var vv = song.Tags?.FirstOrDefault(x => x.Identifier == metaTagIdentifier)?.Value;
        try
        {
            if (vv == null)
            {
                return d;
            }

            if (vv is JsonElement)
            {
                vv = vv.ToString() ?? string.Empty;
            }

            var converter = TypeDescriptor.GetConverter(tType);
            if (tType == typeof(short) || tType == typeof(short?))
            {
                var vvv = short.Parse(vv?.ToString() ?? string.Empty);
                return (T?)(object)vvv;
            }

            if (tType == typeof(int) || tType == typeof(int?))
            {
                var vvv = int.Parse(vv?.ToString() ?? string.Empty);
                return (T?)(object)vvv;
            }

            if (tType == typeof(Guid) || tType == typeof(Guid?))
            {
                var g = Guid.Parse(vv.ToString() ?? string.Empty);
                return g == Guid.Empty && tType == typeof(Guid?) ? d : (T?)converter.ConvertFrom(g);
            }

            if (vv is DateTime && tType == typeof(string))
            {
                return (T?)converter.ConvertFrom(vv.ToString() ?? string.Empty);
            }

            if (tType == typeof(DateTime) || tType == typeof(DateTime?))
            {
                var dt = DateTime.Parse(vv.ToString() ?? string.Empty, CultureInfo.InvariantCulture);
                return dt == DateTime.MinValue ? d : (T)Convert.ChangeType(vv, tType);
            }

            if ((tType == typeof(string) && vv is int) || vv is int?)
            {
                return (T?)converter.ConvertFrom(vv?.ToString() ?? string.Empty);
            }

            if (vv is T)
            {
                return (T?)vv;
            }

            return (T?)converter.ConvertFrom(vv);
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Song [{song}] MetaTagIdentifier [{metaTagIdentifier.ToString()}] Value [{vv}] to type [{tType}] [{e}]");
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

    public static string? Comment(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.Comment);
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

    public static long? SongArtistAlbumUniqueId(this Song song)
    {
        return SafeParser.Hash(song.AlbumArtist().ToNormalizedString() ?? song.AlbumArtist() ?? string.Empty,
            song.MediaNumber().ToString(),
            song.AlbumTitle() ?? string.Empty);
    }

    public static long? SongUniqueId(this Song song)
    {
        return SafeParser.Hash(song.AlbumArtist().ToNormalizedString() ?? song.AlbumArtist() ?? string.Empty,
            song.MediaNumber().ToString(),
            song.SongNumber().ToString(),
            song.Title().ToNormalizedString() ?? song.Title());
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
        return (song.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
                song.MetaTagValue<int?>(MetaTagIdentifier.OrigAlbumDate) ??
                song.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear)) ?? SafeParser.ToDateTime(song.AlbumDate())?.Year;
    }

    public static string? AlbumDate(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.Date) ??
               song.MetaTagValue<string?>(MetaTagIdentifier.AlbumDate) ??
               song.MetaTagValue<string?>(MetaTagIdentifier.OrigAlbumDate) ??
               song.MetaTagValue<string?>(MetaTagIdentifier.RecordingDateOrYear);
    }

    /// <summary>
    /// Returns song duration in Milliseconds
    /// </summary>
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
        var albumDate = song.AlbumDate();
        var year = SafeParser.ToNumber<int?>(albumDate);
        if (year != null)
        {
            return DateTime.Parse($"01/01/{year}", CultureInfo.InvariantCulture);
        }

        return albumDate.Nullify() == null ? null : DateTime.Parse(albumDate!, CultureInfo.InvariantCulture);
    }

    public static short MediaNumber(this Song song)
    {
        var mediaNumber = song.MetaTagValue<short?>(MetaTagIdentifier.DiscNumber) ?? 1;
        return mediaNumber < 1 ? (short)1 : mediaNumber;
    }

    public static int SongNumber(this Song song)
    {
        return song.MetaTagValue<int?>(MetaTagIdentifier.TrackNumber) ?? 0;
    }

    public static short SongTotalNumber(this Song song)
    {
        return song.MetaTagValue<short?>(MetaTagIdentifier.SongTotal) ?? 0;
    }

    public static string? MediaSubTitle(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.SubTitle);
    }

    public static string? Genre(this Song song)
    {
        return song.MetaTagValue<string?>(MetaTagIdentifier.Genre);
    }

    public static int BitRate(this Song song)
    {
        return SafeParser.ToNumber<int>(song?.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitRate)?.Value);
    }

    public static int BitDepth(this Song song)
    {
        return SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitDepth)?.Value);
    }

    public static int? ChannelCount(this Song song)
    {
        return SafeParser.ToNumber<int?>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.Channels)?.Value);
    }

    public static bool IsVbr(this Song song)
    {
        return SafeParser.ToNumber<bool>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.IsVbr)?.Value);
    }

    public static int SamplingRate(this Song song)
    {
        return SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.SampleRate)?.Value);
    }

    public static bool IsValid(this Song song, Dictionary<string, object?> configuration)
    {
        if (song.Tags?.Count() == 0)
        {
            return false;
        }

        var songNumber = song.SongNumber();
        return songNumber > 0 &&
               songNumber < SafeParser.ToNumber<int>(configuration[SettingRegistry.ValidationMaximumSongNumber]) &&
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

    public static string ContentType(this Song song)
    {
        return MimeTypes.GetMimeType(song.File.Name);
    }

    public static string FileExtension(this Song song)
    {
        return Path.GetExtension(song.File.Name);
    }

    public static string SongFileName(
        FileInfo fileInfo,
        int songNumber,
        string? songTitle,
        string? extension = null)
    {
        if (songNumber < 1)
        {
            Trace.WriteLine($"File [{fileInfo}] has invalid Song number [{songNumber}]");
            songNumber = 0;
        }

        if (string.IsNullOrWhiteSpace(songTitle))
        {
            Trace.WriteLine($"File [{fileInfo}] has invalid Song title [{songTitle}]");
            songTitle = fileInfo.Name;
        }

        var songNumberValue = songNumber.ToStringPadLeft(MelodeeConfiguration.SongFileNameNumberPadding);
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

        return $"{songNumberValue} {fileNameFromTitle}{extension ?? fileInfo.Extension}";
    }

    public static string ToSongFileName(this Song song, FileSystemDirectoryInfo directoryInfo)
    {
        return SongFileName(
            song.File.ToFileInfo(directoryInfo),
            song.SongNumber(),
            song.Title());
    }

    private static MetaTagIdentifier[] ContributorMetaTagIdentifiers =>
    [
        MetaTagIdentifier.Artist,
        MetaTagIdentifier.Composer,
        MetaTagIdentifier.Conductor,
        MetaTagIdentifier.Engineer,
        MetaTagIdentifier.InterpretedRemixedOrOtherwiseModifiedBy,
        MetaTagIdentifier.Lyricist,
        MetaTagIdentifier.MixDj,
        MetaTagIdentifier.MixEngineer,
        MetaTagIdentifier.MusicianCredit,
        MetaTagIdentifier.OriginalArtist,
        MetaTagIdentifier.OriginalLyricist,
        MetaTagIdentifier.Producer
    ];

    public static async Task<Melodee.Common.Data.Models.Contributor[]> GetContributorsForSong(this Song song,
        Instant now,
        ArtistService artistService,
        int artistId,
        int albumId,
        int songId,
        string[] ignorePerformers,
        string[] ignoreProduction,
        string[] ignorePublishers,
        CancellationToken token)
    {
        var dbContributorsToAdd = new List<Melodee.Common.Data.Models.Contributor>();
        foreach (var contributorTag in ContributorMetaTagIdentifiers)
        {
            var tagValue = song.MetaTagValue<string?>(contributorTag)?.CleanStringAsIs();
            if (!dbContributorsToAdd.Any(x => (x.ArtistId == artistId || x.ContributorName == tagValue) && x.MetaTagIdentifierValue == contributorTag))
            {
                var contributorForTag = await CreateContributorForSongAndTag(song,
                    artistService,
                    contributorTag,
                    albumId,
                    songId,
                    now,
                    null,
                    null,
                    ignorePerformers,
                    ignoreProduction,
                    ignorePublishers,
                    token);
                if (contributorForTag != null)
                {
                    dbContributorsToAdd.Add(contributorForTag);
                }
            }
        }

        foreach (var tmclTag in song.Tags?.Where(x => x.Value != null && x.Value.ToString()!.StartsWith("TMCL:", StringComparison.OrdinalIgnoreCase)) ?? [])
        {
            var subRole = tmclTag.Value!.ToString()!.Substring(6).Trim();
            var tagValue = song.MetaTagValue<string?>(tmclTag.Identifier)?.CleanStringAsIs();
            if (!dbContributorsToAdd.Any(x => (x.ArtistId == artistId || x.ContributorName == tagValue) &&
                                              x.MetaTagIdentifierValue == tmclTag.Identifier))
            {
                var contributorForTag = await CreateContributorForSongAndTag(song,
                    artistService,
                    tmclTag.Identifier,
                    albumId,
                    songId,
                    now,
                    subRole,
                    null,
                    ignorePerformers,
                    ignoreProduction,
                    ignorePublishers,
                    token);
                if (contributorForTag != null)
                {
                    dbContributorsToAdd.Add(contributorForTag);
                }
            }
        }

        var songPublisherTag = song.MetaTagValue<string?>(MetaTagIdentifier.Publisher);
        if (songPublisherTag != null)
        {
            var publisherName = songPublisherTag.CleanStringAsIs();
            if (!dbContributorsToAdd.Any(x => x.ContributorName == publisherName && x.MetaTagIdentifierValue == MetaTagIdentifier.Publisher))
            {
                var publisherTag = await CreateContributorForSongAndTag(song,
                    artistService,
                    MetaTagIdentifier.Publisher,
                    albumId,
                    null,
                    now,
                    null,
                    publisherName,
                    ignorePerformers,
                    ignoreProduction,
                    ignorePublishers,
                    token);
                if (publisherTag != null && dbContributorsToAdd.All(x => x.ContributorTypeValue != ContributorType.Publisher))
                {
                    dbContributorsToAdd.Add(publisherTag);
                }
            }
        }

        return dbContributorsToAdd.ToArray();
    }

    private static async Task<Melodee.Common.Data.Models.Contributor?> CreateContributorForSongAndTag(this Song song,
        ArtistService artistService,
        MetaTagIdentifier tag,
        int dbAlbumId,
        int? dbSongId,
        Instant now,
        string? subRole,
        string? contributorName,
        string[] ignorePerformers,
        string[] ignoreProduction,
        string[] ignorePublishers,
        CancellationToken cancellationToken = default)
    {
        var contributorNameValue = contributorName.Nullify()?.CleanStringAsIs() ?? song.MetaTagValue<string?>(tag)?.CleanStringAsIs();
        if (contributorNameValue.Nullify() != null)
        {
            var artist = contributorNameValue == null ? null : await artistService.GetByNameNormalized(contributorNameValue.ToNormalizedString() ?? contributorName!, cancellationToken).ConfigureAwait(false);
            var contributorType = DetermineContributorType(tag);
            if (DoMakeContributorForTageTypeAndValue(ignorePerformers, ignoreProduction, ignorePublishers, contributorType, contributorNameValue))
            {
                return new Melodee.Common.Data.Models.Contributor
                {
                    AlbumId = dbAlbumId,
                    ArtistId = artist?.Data?.Id,
                    ContributorName = contributorNameValue,
                    ContributorType = SafeParser.ToNumber<int>(contributorType),
                    CreatedAt = now,
                    MetaTagIdentifier = SafeParser.ToNumber<int>(tag),
                    Role = tag.GetEnumDescriptionValue(),
                    SongId = dbSongId,
                    SubRole = subRole?.CleanStringAsIs()
                };
            }
        }

        return null;
    }

    private static bool DoMakeContributorForTageTypeAndValue(string[] ignorePerformers, string[] ignoreProduction, string[] ignorePublishers, ContributorType type, string? contributorName)
    {
        switch (type)
        {
            case ContributorType.Performer:
                if (ignorePerformers.Contains(contributorName.ToNormalizedString()))
                {
                    return false;
                }

                break;

            case ContributorType.Production:
                if (ignoreProduction.Contains(contributorName.ToNormalizedString()))
                {
                    return false;
                }

                break;

            case ContributorType.Publisher:
                if (ignorePublishers.Contains(contributorName.ToNormalizedString()))
                {
                    return false;
                }

                break;
        }

        return true;
    }

    private static ContributorType DetermineContributorType(MetaTagIdentifier tag)
    {
        switch (tag)
        {
            case MetaTagIdentifier.AlbumArtist:
            case MetaTagIdentifier.Artist:
            case MetaTagIdentifier.Artists:
            case MetaTagIdentifier.Composer:
            case MetaTagIdentifier.Conductor:
            case MetaTagIdentifier.MusicianCredit:
            case MetaTagIdentifier.OriginalArtist:
            case MetaTagIdentifier.OriginalLyricist:
                return ContributorType.Performer;

            case MetaTagIdentifier.EncodedBy:
            case MetaTagIdentifier.Engineer:
            case MetaTagIdentifier.Group:
            case MetaTagIdentifier.InterpretedRemixedOrOtherwiseModifiedBy:
            case MetaTagIdentifier.InvolvedPeople:
            case MetaTagIdentifier.Lyricist:
            case MetaTagIdentifier.MixDj:
            case MetaTagIdentifier.MixEngineer:
            case MetaTagIdentifier.Producer:
                return ContributorType.Production;

            case MetaTagIdentifier.Publisher:
                return ContributorType.Publisher;
        }

        return ContributorType.NotSet;
    }
}
