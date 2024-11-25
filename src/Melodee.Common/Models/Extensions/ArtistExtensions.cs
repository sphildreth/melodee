using Melodee.Common.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models.Extensions;

public static class ArtistExtensions
{
    public static long UniqueId(this Artist artist) => SafeParser.Hash(artist.MusicBrainzId ?? artist.NameNormalized);
    
    public static bool IsValid(this Artist artist) => artist.UniqueId() > 0 && artist.Name.Nullify() != null;

    public static string ToDirectoryName(this Artist artist, int processingMaximumArtistDirectoryNameLength)
    {
        var artistNameToUse = artist.SortName ?? artist.Name;
        if (string.IsNullOrWhiteSpace(artistNameToUse))
        {
            throw new Exception("Neither Artist or ArtistSort tag is set.");
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
        var fnIdPart = $" [{artist.UniqueId().ToString()}]";
        var maxFnLength = processingMaximumArtistDirectoryNameLength - (fnSubPart.Length + fnIdPart.Length) - 2;
        if (artistDirectory.Length > maxFnLength)
        {
            artistDirectory = artistDirectory.Substring(0, maxFnLength);
        }
        return Path.Combine(fnSubPart, $"{artistDirectory}{fnIdPart}");
    }
}
