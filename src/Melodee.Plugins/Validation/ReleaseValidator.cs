using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Plugins.Validation.Models;
using Serilog;

namespace Melodee.Plugins.Validation;

public sealed partial class ReleaseValidator(Configuration configuration) : IReleaseValidator
{
    private static readonly Regex UnwantedReleaseTitleTextRegex = new(@"(\s*(-\s)*((CD[_\-#\s]*[0-9]*)))|(\s[\[\(]*(lp|ep|bonus|release|re(\-*)issue|re(\-*)master|re(\-*)mastered|anniversary|single|cd|disc|deluxe|digipak|digipack|vinyl|japan(ese)*|asian|remastered|limited|ltd|expanded|(re)*\-*edition|web|\(320\)|\(*compilation\)*)+(]|\)*))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex UnwantedTrackTitleTextRegex = new(@"(\s{2,}|(\s\(prod\s))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static readonly Regex HasFeatureFragmentsRegex = new(@"(\s[\(\[]*ft[\s\.]|\s*[\(\[]*with\s+|\s*[\(\[]*feat[\s\.]|[\(\[]*(featuring))+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ImageNameIsProofRegex = new(@"(proof)+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

    private readonly Configuration _configuration = configuration;
    private readonly List<ValidationResultMessage> _validationMessages = [];

    public OperationResult<ValidationResult> ValidateRelease(Release? release)
    {
        if (release == null)
        {
            return new OperationResult<ValidationResult>(["Release is invalid."])
            {
                Data = new ValidationResult
                {
                    ReleaseStatus = ReleaseStatus.Invalid
                }
            };
        }
        _validationMessages.Clear();

        var returnStatus = release.Status;

        // Validations should return true if ok
        if (IsValid(release) &&
            AreAllTrackNumbersValid(release) &&
            AreTracksUniquelyNumbered(release) &&
            AreMediaNumbersValid(release) &&
            DoAllTracksHaveSameReleaseArtist(release) &&
            AllTrackTitlesDoNotHaveUnwantedText(release) &&
            AlbumArtistDoesNotHaveUnwantedText(release) &&
            ReleaseTitleDoesNotHaveUnwantedText(release) &&
            IsReleaseYearValid(release) &&
            DoMediaTotalMatchMediaNumbers(release) &&
            DoesTrackTotalMatchTrackCount(release)
           )
        {
            returnStatus = ReleaseStatus.Ok;
        }

        return new OperationResult<ValidationResult>
        {
            Data = new ValidationResult
            {
                Messages = _validationMessages,
                ReleaseStatus = returnStatus
            }
        };
    }

    private bool DoesTrackTotalMatchTrackCount(Release release)
    {
        var result = true;
        var tracks = release.Tracks?.ToArray() ?? [];
        if (tracks.Length == 0)
        {
            result = false;
        }
        var trackCount = tracks.Length;
        var trackTotal = release.TrackTotalValue();
        result = trackCount == trackTotal;
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{release}' release track total value does not match track count.",
                Severity = ValidationResultMessageSeverity.MustFix
            });
        }
        return result; 
    }

    private bool DoMediaTotalMatchMediaNumbers(Release release)
    {
        var result = true;
        var tracks = release.Tracks?.ToArray() ?? [];
        if (tracks.Length == 0)
        {
            result = false;
        }
        var mediaNumbers = tracks.Select(x => x.MediaNumber()).Distinct().ToArray();
        var releaseMediaTotal = release.MediaCountValue();
        result = mediaNumbers.All(mediaNumber => mediaNumber <= releaseMediaTotal);
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{release}' release media total [{releaseMediaTotal}] does not match track medias [{mediaNumbers}].",
                Severity = ValidationResultMessageSeverity.MustFix
            });
        }
        return result;        
    }

    private bool IsValid(Release release)
    {
        var result = true;
        if (!release.IsValid(_configuration))
        {
            if (release.UniqueId < 0)
            {
                _validationMessages.Add(new ValidationResultMessage
                {
                    Message = $"Release has invalid Unique ID: {release.UniqueId}",
                    Severity = ValidationResultMessageSeverity.MustFix
                });
                result = false;
            }
            if (release.Artist().Nullify() == null)
            {
                _validationMessages.Add(new ValidationResultMessage
                {
                    Message = $"Release has invalid Artist [{release.Artist()}]",
                    Severity = ValidationResultMessageSeverity.MustFix
                });
                result = false;
            }
            if (release.ReleaseTitle().Nullify() == null)
            {
                _validationMessages.Add(new ValidationResultMessage
                {
                    Message = $"Release has invalid Release Title: {release.ReleaseTitle()}",
                    Severity = ValidationResultMessageSeverity.MustFix
                });
                result = false;
            }

            if (!release.HasValidReleaseYear(_configuration))
            {
                _validationMessages.Add(new ValidationResultMessage
                {
                    Message = $"Release has invalid Release Year: {release.ReleaseYear()}",
                    Severity = ValidationResultMessageSeverity.MustFix
                });
                result = false;
            }
        }
        return result;
    }
    
    private bool AreTracksUniquelyNumbered(Release release)
    {
        var tracks = release.Tracks?.ToArray() ?? [];
        if (tracks.Length == 0)
        {
            return false;
        }

        var trackNumbers = tracks.GroupBy(x => x.TrackNumber());
        var result =  trackNumbers.All(group => group.Count() == 1);
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{release}' release has tracks with invalid track number.",
                Severity = ValidationResultMessageSeverity.MustFix
            });
        }

        return result;
    }

    /// <summary>
    ///     Check if all the tracks have the same Album Artist. This is an issue if not a VA type release.
    /// </summary>
    private bool DoAllTracksHaveSameReleaseArtist(Release release)
    {
        var result = true;
        var tracks = release.Tracks?.ToArray() ?? [];
        if (tracks.Length == 0)
        {
            result = false;
        }

        var albumArtist = release.Artist();
        if (string.IsNullOrWhiteSpace(albumArtist))
        {
            result = false;
        }

        if (result)
        {
            var tracksGroupedByArtist = tracks.GroupBy(x => x.ReleaseArtist()).ToArray();
            result = tracksGroupedByArtist.First().Key.Nullify() == null ||
                     (string.Equals(tracksGroupedByArtist.First().Key, albumArtist) &&
                      tracksGroupedByArtist.Length == 1);
        }

        if (!result && !release.IsVariousArtistTypeRelease())
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{release}' tracks do not all have the same album artist.",
                Severity = ValidationResultMessageSeverity.MustFix
            });
        }

        return result;
    }

    private bool AlbumArtistDoesNotHaveUnwantedText(Release release)
    {
        var result = !StringHasFeaturingFragments(release.Artist());
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{release}' release artist has unwanted text.",
                Severity = ValidationResultMessageSeverity.Undesired
            });
        }

        return result;
    }

    private bool ReleaseTitleDoesNotHaveUnwantedText(Release release)
    {
        var result = !StringHasFeaturingFragments(release.ReleaseTitle());
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{release}' release title has unwanted text.",
                Severity = ValidationResultMessageSeverity.Undesired
            });
        }

        return result;
    }

    private bool AreMediaNumbersValid(Release release)
    {
        var result = true;
        var tracks = release.Tracks?.ToArray() ?? [];
        if (tracks.Length == 0)
        {
            result = false;
        }

        var mediaNumbers = tracks.Select(x => x.MediaNumber()).Distinct().ToArray();
        if (mediaNumbers.Length != 0 && mediaNumbers.All(x => x > 0))
        {
            if (mediaNumbers.Any(x => x > _configuration.ValidationOptions.MaximumMediaNumber) || 
                mediaNumbers.Any(x => x < _configuration.ValidationOptions.MinimumMediaNumber))
            {
                result = false;
            }
            result = Enumerable.Range(0, mediaNumbers.Length).All(i => mediaNumbers[i] == mediaNumbers[0] + i);
        }

        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{release}' release media numbers are invalid.",
                Severity = ValidationResultMessageSeverity.MustFix
            });
        }

        return result;
    }

    private bool AllTrackTitlesDoNotHaveUnwantedText(Release release)
    {
        var result = true;
        var tracks = release.Tracks?.ToArray() ?? [];
        if (tracks.Length == 0)
        {
            result = false;
        }

        foreach (var track in tracks)
        {
            if (TrackHasUnwantedText(release.ReleaseTitle(), track.Title(), track.TrackNumber()))
            {
                result = false;
                break;
            }
        }

        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{release}' some tracks have unwanted text.",
                Severity = ValidationResultMessageSeverity.Undesired
            });
        }

        return result;
    }


    private bool AreAllTrackNumbersValid(Release release)
    {
        var result = true;
        var tracks = release.Tracks?.ToArray() ?? [];
        if (tracks.Length == 0)
        {
            result = false;
        }

        var trackNumbers = tracks.Select(x => x.TrackNumber()).Distinct().ToArray();
        if (trackNumbers.Length == 0)
        {
            result = false;
        }

        if (trackNumbers.Contains(0))
        {
            result = false;
        }

        if (trackNumbers.Any(x => x > _configuration.ValidationOptions.MaximumTrackNumber))
        {
            result = false;
        }
        result = result && Enumerable.Range(0, trackNumbers.Length).All(i => trackNumbers[i] == trackNumbers[0] + i);
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{release}' release track numbers are invalid.",
                Severity = ValidationResultMessageSeverity.MustFix
            });
        }
        return result;
    }

    private bool IsReleaseYearValid(Release release)
    {
        var result = release.HasValidReleaseYear(_configuration);
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{release}' release year is invalid.",
                Severity = ValidationResultMessageSeverity.MustFix
            });
            
        }
        return result;
    }

    public static bool StringHasFeaturingFragments(string? input)
    {
        return !string.IsNullOrWhiteSpace(input) && HasFeatureFragmentsRegex.IsMatch(input);
    }

    public static bool IsImageAProofType(string? imageName)
    {
        return !string.IsNullOrWhiteSpace(imageName) && ImageNameIsProofRegex.IsMatch(imageName);
    }

    public static string? ReplaceTrackArtistSeparators(string? trackArtist)
    {
        if (trackArtist.Nullify() == null)
        {
            return null;
        }
        return ReplaceTrackArtistSeparatorsRegex().Replace(trackArtist!, "/").Trim();
    }    
    
    public static bool ReleaseTitleHasUnwantedText(string? releaseTitle)
    {
        return string.IsNullOrWhiteSpace(releaseTitle) || UnwantedReleaseTitleTextRegex.IsMatch(releaseTitle);
    }

    public static string? RemoveUnwantedTextFromReleaseTitle(string? title)
    {
        if (title.Nullify() == null)
        {
            return null;
        }
        return UnwantedReleaseTitleTextRegex.Replace(title!, string.Empty).Trim();
    }    

    public static bool TrackHasUnwantedText(string? releaseTitle, string? trackTitle, int? trackNumber)
    {
        if (string.IsNullOrWhiteSpace(trackTitle))
        {
            return true;
        }

        if (StringHasFeaturingFragments(trackTitle))
        {
            return true;
        }

        try
        {
            if (UnwantedTrackTitleTextRegex.IsMatch(trackTitle))
            {
                return true;
            }

            if (trackTitle.Any(char.IsDigit))
            {
                if (string.Equals(trackTitle.Trim(), (trackNumber ?? 0).ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return Regex.IsMatch(trackTitle, $@"^({Regex.Escape(releaseTitle ?? string.Empty)}\s*.*\s*)?([0-9]*{trackNumber}\s)");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "TrackHasUnwantedText For ReleaseTitle [{releaseTitle}] for TrackTitle [{trackTitle}]", releaseTitle, trackTitle);
        }

        return false;
    }

    [GeneratedRegex(@"\s+with\s+|\s*;\s*|\s*(&|ft(\.)*|feat)\s*|\s+x\s+|\s*\,\s*", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ReplaceTrackArtistSeparatorsRegex();
}
