using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;
using Serilog;

namespace Melodee.Common.Models.Extensions;

public static class ReleaseExtensions
{
    private static readonly IEnumerable<string> FolderSpaceReplacements = new List<string> { ".", "~", "_", "=", "-" };
    
    private static readonly Regex UnwantedReleaseTitleTextRegex = new(@"(\s*(-\s)*((CD[_\-#\s]*[0-9]*)))|(\s[\[\(]*(lp|ep|bonus|release|re(\-*)issue|re(\-*)master|re(\-*)mastered|anniversary|single|cd|disc|deluxe|digipak|digipack|vinyl|japan(ese)*|asian|remastered|limited|ltd|expanded|(re)*\-*edition|web|\(320\)|\(*compilation\)*)+(]|\)*))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            if (vv is JsonElement)
            {
                vv = vv.ToString() ?? string.Empty;
            }
            return (T?)converter.ConvertFrom(vv);           
        }
        catch (Exception e)
        {
            Log.Error(e, "Release [{Release}", release);
        }
        return d;
    }
    
    public static bool IsValid(this Release release)
    {
        if (release.Tags?.Count() == 0)
        {
            return false;
        }

        var artist = release.Artist().Nullify();
        var releaseTitle = release.ReleaseTitle().Nullify();
        var releaseYear = release.ReleaseYear();
        return artist != null &&
               releaseTitle != null &&
               releaseYear > DateTime.MinValue.Year && releaseYear < DateTime.MaxValue.Year;
    }
    
    public static string ToMelodeeJsonName(this Release release) => $"{(release.Artist() ?? throw new Exception("Artist not set on release.")).ToFileNameFriendly()}_{(release.ReleaseTitle() ?? throw new Exception("Title not set on release.")).ToFileNameFriendly()}.melodee.json";

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
    
    
    /// <summary>
    /// Return the value set for the Artist Tag.
    /// </summary>
    public static string? Artist(this Release release) => release.MetaTagValue<string?>(MetaTagIdentifier.Artist);

    public static long ArtistUniqueId(this Release release) => release.MetaTagValue<long?>(MetaTagIdentifier.UniqueArtistId) ?? SafeParser.Hash(release.ArtistSort() ?? release.Artist() ?? Guid.NewGuid().ToString());

    public static string? ArtistSort(this Release release) => release.MetaTagValue<string?>(MetaTagIdentifier.SortAlbumArtist);
    
    /// <summary>
    /// Return the value set for the Album Tag.
    /// </summary>
    public static string? ReleaseTitle(this Release release) => release.MetaTagValue<string?>(MetaTagIdentifier.Album);
    
    /// <summary>
    /// Return the value set for the OrigReleaseYear ?? RecordingYear ?? RecordingDateOrYear
    /// </summary>
    public static int? ReleaseYear(this Release release) => release.MetaTagValue<int?>(MetaTagIdentifier.OrigReleaseYear) ?? 
                                                            release.MetaTagValue<int?>(MetaTagIdentifier.RecordingYear) ??
                                                            release.MetaTagValue<int?>(MetaTagIdentifier.RecordingDateOrYear);
    public static int MediaCountValue(this Release release) => release.MetaTagValue<int?>(MetaTagIdentifier.DiscNumberTotal) ?? 
                                                               release.MetaTagValue<int?>(MetaTagIdentifier.DiscTotal) ?? 
                                                               0;

    public static int TrackTotalValue(this Release release)
    {
        var trackTotalFromRelease = release.MetaTagValue<int?>(MetaTagIdentifier.TrackTotal);
        if (trackTotalFromRelease is > 0)
        {
            return trackTotalFromRelease.Value;
        }
        var trackTotalFromTracks = release.Tracks?.FirstOrDefault(x => x.TrackTotalNumber() > 0);
        if (trackTotalFromTracks != null)
        {
            return trackTotalFromTracks.TrackTotalNumber();
        }
        return release.Tracks?.Count() ?? 0;
    }
    
    public static string? Genre(this Release release) => release.MetaTagValue<string?>(MetaTagIdentifier.Genre);

    public static bool TitleHasUnwantedText(this Release release)
    {
        var title = release.ReleaseTitle();
        return string.IsNullOrWhiteSpace(title) || UnwantedReleaseTitleTextRegex.IsMatch(title);
    }
    
    public static System.IO.DirectoryInfo ToDirectoryInfo(this Release release) => new System.IO.DirectoryInfo(release.OriginalDirectory.Path);
   
    public static IEnumerable<System.IO.FileInfo> FileInfosForExtension(this Release release, string extension)
    {
        var dirInfo = release.ToDirectoryInfo();
        if (!dirInfo.Exists)
        {
            return [];
        }
        return dirInfo.GetFiles($"*.{extension}", SearchOption.AllDirectories).ToArray();
    }

    /// <summary>
    /// Is the given File related to this release.
    /// </summary>
    public static bool IsFileForRelease(this Release release, FileSystemInfo fileSystemInfo)
    {
        if (!fileSystemInfo.Exists)
        {
            return false;
        }

        if (release.Files.Any())
        {
            var fileSystemInfoJustName = Path.GetFileNameWithoutExtension(fileSystemInfo.FullName).ToAlphanumericName();
            if (release.Files.Select(releaseFile => releaseFile.FileNameNoExtension().ToAlphanumericName())
                .Any(releaseFileNameJustName => releaseFileNameJustName.DoStringsMatch(fileSystemInfoJustName)))
            {
                return true;
            }
        }
        
        var normalizedName = fileSystemInfo.Name.ToAlphanumericName();
        if (string.IsNullOrEmpty(normalizedName))
        {
            return false;
        }
        try
        {
            var normalizedArtistName = release.Artist()?.ToAlphanumericName() ?? string.Empty;
            var normalizedTitleName = release.ReleaseTitle()?.ToAlphanumericName() ?? string.Empty;

            if (normalizedName.Contains(normalizedArtistName) && normalizedName.Contains(normalizedTitleName))
            {
                return true;
            }

            if (release.Tracks != null)
            {
                foreach (var track in release.Tracks)
                {
                    var normalizedTrackArtist = track.TrackArtist()?.ToAlphanumericName() ?? String.Empty;
                    var normalizedTrackName = track.Title()?.ToAlphanumericName() ?? string.Empty;
                    if ((normalizedName.Contains(normalizedArtistName) || normalizedName.Contains(normalizedTrackArtist)) && 
                         normalizedName.Contains(normalizedTrackName))
                    {
                        return true;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Error trying to determine if file [{fileSystemInfo.FullName}] is for Release [{release}]", "Error");
        }
        return false;
    }

    public static string ReleaseDirectoryName(this Release release, Configuration.Configuration configuration)
    {
        var releaseTitleValue = release.ReleaseTitle();
        if (string.IsNullOrWhiteSpace(releaseTitleValue))
        {
            throw new Exception("Unable to determine Release title from Release.");
        }
        var releaseTitle = releaseTitleValue!;
        var releasePathTitle = releaseTitle.ToAlphanumericName(false, false)?.ToFolderNameFriendly()?.ToTitleCase(false);
        if (string.IsNullOrEmpty(releasePathTitle))
        {
            throw new Exception($"Unable to determine Release Path for Release [{ release }].");
        }
        var maxFnLength = configuration.PluginProcessOptions.MaximumReleaseDirectoryNameLength - 7;
        if (releasePathTitle.Length > maxFnLength)
        {
            releasePathTitle = releasePathTitle.Substring(0, maxFnLength);
        }

        var releaseDate = release.ReleaseYear();
        if (releaseDate.HasValue && releaseDate < configuration.PluginProcessOptions.MinimumValidReleaseYear)
        {
            throw new Exception($"Invalid year [{releaseDate}] for release [{release}], Minimum configured value is [{configuration.PluginProcessOptions.MinimumValidReleaseYear}]");
        }
        return Path.Combine(release.ArtistDirectoryName(configuration), $"[{ releaseDate}] {releasePathTitle}");
    }
    
    public static string ArtistDirectoryName(this Release release, Configuration.Configuration configuration)
    {
        var artistNameToUse = release.ArtistSort() ?? release.Artist();
        if (string.IsNullOrWhiteSpace(artistNameToUse))
        {
            throw new Exception("Neither Artist or ArtistSort tag is set on Release.");
        }
        var artistFolder = artistNameToUse!.ToAlphanumericName(false, false)?.ToFolderNameFriendly()?.ToTitleCase(false);
        if (string.IsNullOrEmpty(artistFolder))
        {
            throw new Exception($"Unable to determine artist folder for releae ArtistNameToUse [{ artistNameToUse }].");
        }
        var afUpper = artistFolder.ToUpper();
        var fnSubPart1 = afUpper.ToUpper().ToCharArray().Take(1).First();
        if (!char.IsLetterOrDigit(fnSubPart1))
        {
            fnSubPart1 = '#';
        }
        else if (char.IsNumber(fnSubPart1))
        {
            fnSubPart1 = '0';
        }
        var fnSubPart2 = afUpper.Length > 2 ? afUpper.Substring(0, 2) : afUpper;
        if (fnSubPart2.EndsWith(" "))
        {
            var pos = 1;
            while (fnSubPart2.EndsWith(" "))
            {
                pos++;
                fnSubPart2 = fnSubPart2.Substring(0, 1) + afUpper.Substring(pos, 1);
            }
        }
        var fnSubPart = Path.Combine(fnSubPart1.ToString(), fnSubPart2);
        var fnIdPart = $" [{ release.ArtistUniqueId() }]";
        var maxFnLength = (configuration.PluginProcessOptions.MaximumArtistDirectoryNameLength - (fnSubPart.Length + fnIdPart.Length)) - 2;
        if (artistFolder.Length > maxFnLength)
        {
            artistFolder = artistFolder.Substring(0, maxFnLength);
        }
        return Path.Combine(fnSubPart, $"{ artistFolder }{ fnIdPart }");
    }
}