using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Utility;

namespace Melodee.Common.Models.Extensions;

public static class ArtistExtensions
{
    public static readonly Regex VariousArtistParseRegex = new(@"([\[\(]*various\s*artists[\]\)]*)|([\[\(]*va[\]\)]*(\W))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static readonly Regex CastRecordingSongArtistParseRegex = new(@"(original broadway cast|original cast*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static KeyValue ToKeyValue(this Artist artist)
    {
        return new KeyValue(artist.ArtistDbId?.ToString() ?? artist.MusicBrainzId?.ToString() ?? artist.Name.ToNormalizedString() ?? artist.Name, artist.Name.ToNormalizedString() ?? artist.Name);
    }

    public static ArtistQuery ToArtistQuery(this Artist artist, KeyValue[] albumKeyValues)
    {
        return new ArtistQuery
        {
            Name = artist.Name,
            AlbumKeyValues = albumKeyValues,
            MusicBrainzId = artist.MusicBrainzId.ToString()
        };
    }

    public static AlbumArtistType ArtistType(this Artist artist)
    {
        var artistName = artist.Name;
        if (string.IsNullOrWhiteSpace(artistName))
        {
            return AlbumArtistType.NotSet;
        }
        if (artist.IsVariousArtist())
        {
            return AlbumArtistType.VariousArtists;
        }
        return AlbumArtistType.ArtistOrBand;
    }

    public static bool IsValid(this Artist artist)
    {
        return (artist.ArtistDbId != null || artist.MusicBrainzId != null) && artist.Name.Nullify() != null;
    }

    public static string ToAlphanumericName(this Artist artist, bool stripSpaces = true, bool stripCommas = true)
    {
        return artist.Name.ToAlphanumericName(stripSpaces, stripCommas);
    }

    public static bool IsCastRecording(this Artist artist)
    {
        var artistName = artist.Name;
        return artistName.Nullify() != null && CastRecordingSongArtistParseRegex.IsMatch(artistName);
    }

    // public static bool IsSoundSongArist(this Artist artist)
    // {
    //     var artistName = artist.Name;
    //     return artistName.Nullify() != null && SoundSongArtistParseRegex.IsMatch(artistName);
    // }

    public static bool IsVariousArtist(this Artist artist)
    {
        var artistName = artist.Name;
        if (artistName.Nullify() == null)
        {
            return false;
        }

        if (string.Equals(artistName, "va", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return VariousArtistParseRegex.IsMatch(artistName);
    }

    public static string ToDirectoryName(this Artist artist, int processingMaximumArtistDirectoryNameLength)
    {
        var artistNameToUse = artist.SortName ?? artist.Name;
        if (string.IsNullOrWhiteSpace(artistNameToUse))
        {
            throw new Exception("Neither Artist or ArtistSort tag is set.");
        }

        if (artist.ArtistDbId == null && artist.MusicBrainzId == null)
        {
            throw new Exception("Neither ArtistDbId or MusicBrainzId is set.");
        }

        var artistDirectory = artistNameToUse.ToAlphanumericName(false, false).ToDirectoryNameFriendly()?.ToTitleCase(false);
        if (string.IsNullOrEmpty(artistDirectory))
        {
            throw new Exception($"Unable to determine artist directory for Album ArtistNameToUse [{artistNameToUse}].");
        }

        var afUpper = artistDirectory.ToUpper();
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
        if (fnSubPart2.EndsWith(' '))
        {
            var pos = 1;
            while (fnSubPart2.EndsWith(' '))
            {
                pos++;
                fnSubPart2 = fnSubPart2[..1] + afUpper.Substring(pos, 1);
            }
        }

        var fnSubPart = Path.Combine(fnSubPart1.ToString(), fnSubPart2);
        var fnIdPart = $" [{SafeParser.Hash(artist.ArtistDbId.ToString(), artist.MusicBrainzId.ToString())}]";
        var maxFnLength = processingMaximumArtistDirectoryNameLength - (fnSubPart.Length + fnIdPart.Length) - 2;
        if (artistDirectory.Length > maxFnLength)
        {
            artistDirectory = artistDirectory.Substring(0, maxFnLength);
        }

        return Path.Combine(fnSubPart, $"{artistDirectory}{fnIdPart}/");
    }
}
