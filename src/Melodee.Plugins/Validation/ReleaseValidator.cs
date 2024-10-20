using System.Text.RegularExpressions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Plugins.Validation.Models;
using Serilog;

namespace Melodee.Plugins.Validation;

public sealed partial class AlbumValidator(Configuration configuration) : IAlbumValidator
{
    private static readonly Regex UnwantedAlbumTitleTextRegex = new(@"(\s*(-\s)*((CD[_\-#\s]*[0-9]*)))|(\s[\[\(]*(lp|ep|bonus|Album|re(\-*)issue|re(\-*)master|re(\-*)mastered|anniversary|single|cd|disc|deluxe|digipak|digipack|vinyl|japan(ese)*|asian|remastered|limited|ltd|expanded|(re)*\-*edition|web|\(320\)|\(*compilation\)*)+(]|\)*))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex UnwantedSongTitleTextRegex = new(@"(\s{2,}|(\s\(prod\s))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static readonly Regex HasFeatureFragmentsRegex = new(@"(\s[\(\[]*ft[\s\.]|\s*[\(\[]*with\s+|\s*[\(\[]*feat[\s\.]|[\(\[]*(featuring))+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ImageNameIsProofRegex = new(@"(proof)+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

    private readonly Configuration _configuration = configuration;
    private readonly List<ValidationResultMessage> _validationMessages = [];

    public OperationResult<ValidationResult> ValidateAlbum(Album? album)
    {
        if (album == null)
        {
            return new OperationResult<ValidationResult>(["Album is invalid."])
            {
                Data = new ValidationResult
                {
                    AlbumStatus = AlbumStatus.Invalid
                }
            };
        }
        _validationMessages.Clear();

        var returnStatus = album.Status;

        // Validations should return true if ok
        if (IsValid(album) &&
            AreAllSongNumbersValid(album) &&
            AreSongsUniquelyNumbered(album) &&
            AreMediaNumbersValid(album) &&
            DoAllSongsHaveSameAlbumArtist(album) &&
            AllSongTitlesDoNotHaveUnwantedText(album) &&
            AlbumArtistDoesNotHaveUnwantedText(album) &&
            AlbumTitleDoesNotHaveUnwantedText(album) &&
            IsAlbumYearValid(album) &&
            DoMediaTotalMatchMediaNumbers(album) &&
            DoesSongTotalMatchSongCount(album)
           )
        {
            returnStatus = AlbumStatus.Ok;
        }

        return new OperationResult<ValidationResult>
        {
            Data = new ValidationResult
            {
                Messages = _validationMessages,
                AlbumStatus = returnStatus
            }
        };
    }

    private bool DoesSongTotalMatchSongCount(Album album)
    {
        var result = true;
        var songs = album.Songs?.ToArray() ?? [];
        if (songs.Length == 0)
        {
            result = false;
        }
        var songCount = songs.Length;
        var songTotal = album.SongTotalValue();
        result = songCount == songTotal;
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{album}' Album Song total value does not match Song count.",
                Severity = ValidationResultMessageSeverity.MustFix
            });
        }
        return result; 
    }

    private bool DoMediaTotalMatchMediaNumbers(Album album)
    {
        var result = true;
        var songs = album.Songs?.ToArray() ?? [];
        if (songs.Length == 0)
        {
            result = false;
        }
        var mediaNumbers = songs.Select(x => x.MediaNumber()).Distinct().ToArray();
        var albumMediaTotal = album.MediaCountValue();
        result = mediaNumbers.All(mediaNumber => mediaNumber <= albumMediaTotal);
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{album}' Album media total [{albumMediaTotal}] does not match Song medias [{mediaNumbers}].",
                Severity = ValidationResultMessageSeverity.MustFix
            });
        }
        return result;        
    }

    private bool IsValid(Album album)
    {
        var result = true;
        if (!album.IsValid(_configuration))
        {
            if (album.UniqueId < 0)
            {
                _validationMessages.Add(new ValidationResultMessage
                {
                    Message = $"Album has invalid Unique ID: {album.UniqueId}",
                    Severity = ValidationResultMessageSeverity.MustFix
                });
                result = false;
            }
            if (album.Artist().Nullify() == null)
            {
                _validationMessages.Add(new ValidationResultMessage
                {
                    Message = $"Album has invalid Artist [{album.Artist()}]",
                    Severity = ValidationResultMessageSeverity.MustFix
                });
                result = false;
            }
            if (album.AlbumTitle().Nullify() == null)
            {
                _validationMessages.Add(new ValidationResultMessage
                {
                    Message = $"Album has invalid Album Title: {album.AlbumTitle()}",
                    Severity = ValidationResultMessageSeverity.MustFix
                });
                result = false;
            }

            if (!album.HasValidAlbumYear(_configuration))
            {
                _validationMessages.Add(new ValidationResultMessage
                {
                    Message = $"Album has invalid Album Year: {album.AlbumYear()}",
                    Severity = ValidationResultMessageSeverity.MustFix
                });
                result = false;
            }
        }
        return result;
    }
    
    private bool AreSongsUniquelyNumbered(Album album)
    {
        var songs = album.Songs?.ToArray() ?? [];
        if (songs.Length == 0)
        {
            return false;
        }

        var songNumbers = songs.GroupBy(x => x.SongNumber());
        var result =  songNumbers.All(group => group.Count() == 1);
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{album}' Album has Songs with invalid Song number.",
                Severity = ValidationResultMessageSeverity.MustFix
            });
        }

        return result;
    }

    /// <summary>
    ///     Check if all the Songs have the same Album Artist. This is an issue if not a VA type Album.
    /// </summary>
    private bool DoAllSongsHaveSameAlbumArtist(Album album)
    {
        var result = true;
        var songs = album.Songs?.ToArray() ?? [];
        if (songs.Length == 0)
        {
            result = false;
        }

        var albumArtist = album.Artist();
        if (string.IsNullOrWhiteSpace(albumArtist))
        {
            result = false;
        }

        if (result)
        {
            var songsGroupedByArtist = songs.GroupBy(x => x.AlbumArtist()).ToArray();
            result = songsGroupedByArtist.First().Key.Nullify() == null ||
                     (string.Equals(songsGroupedByArtist.First().Key, albumArtist) &&
                      songsGroupedByArtist.Length == 1);
        }

        if (!result && !album.IsVariousArtistTypeAlbum())
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{album}' Songs do not all have the same album artist.",
                Severity = ValidationResultMessageSeverity.MustFix
            });
        }

        return result;
    }

    private bool AlbumArtistDoesNotHaveUnwantedText(Album album)
    {
        var result = !StringHasFeaturingFragments(album.Artist());
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{album}' Album artist has unwanted text.",
                Severity = ValidationResultMessageSeverity.Undesired
            });
        }

        return result;
    }

    private bool AlbumTitleDoesNotHaveUnwantedText(Album album)
    {
        var result = !StringHasFeaturingFragments(album.AlbumTitle());
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{album}' Album title has unwanted text.",
                Severity = ValidationResultMessageSeverity.Undesired
            });
        }

        return result;
    }

    private bool AreMediaNumbersValid(Album album)
    {
        var result = true;
        var songs = album.Songs?.ToArray() ?? [];
        if (songs.Length == 0)
        {
            result = false;
        }

        var mediaNumbers = songs.Select(x => x.MediaNumber()).Distinct().ToArray();
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
                Message = $"'{album}' Album media numbers are invalid.",
                Severity = ValidationResultMessageSeverity.MustFix
            });
        }

        return result;
    }

    private bool AllSongTitlesDoNotHaveUnwantedText(Album album)
    {
        var result = true;
        var songs = album.Songs?.ToArray() ?? [];
        if (songs.Length == 0)
        {
            result = false;
        }

        foreach (var song in songs)
        {
            if (SongHasUnwantedText(album.AlbumTitle(), song.Title(), song.SongNumber()))
            {
                result = false;
                break;
            }
        }

        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{album}' some Songs have unwanted text.",
                Severity = ValidationResultMessageSeverity.Undesired
            });
        }

        return result;
    }


    private bool AreAllSongNumbersValid(Album album)
    {
        var result = true;
        var songs = album.Songs?.ToArray() ?? [];
        if (songs.Length == 0)
        {
            result = false;
        }

        var songNumbers = songs.Select(x => x.SongNumber()).Distinct().ToArray();
        if (songNumbers.Length == 0)
        {
            result = false;
        }

        if (songNumbers.Contains(0))
        {
            result = false;
        }

        if (songNumbers.Any(x => x > _configuration.ValidationOptions.MaximumSongNumber))
        {
            result = false;
        }
        result = result && Enumerable.Range(0, songNumbers.Length).All(i => songNumbers[i] == songNumbers[0] + i);
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{album}' Album Song numbers are invalid.",
                Severity = ValidationResultMessageSeverity.MustFix
            });
        }
        return result;
    }

    private bool IsAlbumYearValid(Album album)
    {
        var result = album.HasValidAlbumYear(_configuration);
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"'{album}' Album year is invalid.",
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

    public static string? ReplaceSongArtistSeparators(string? songArtist)
    {
        if (songArtist.Nullify() == null)
        {
            return null;
        }
        return ReplaceSongArtistSeparatorsRegex().Replace(songArtist!, "/").Trim();
    }    
    
    public static bool AlbumTitleHasUnwantedText(string? albumTitle)
    {
        return string.IsNullOrWhiteSpace(albumTitle) || UnwantedAlbumTitleTextRegex.IsMatch(albumTitle);
    }

    public static string? RemoveUnwantedTextFromAlbumTitle(string? title)
    {
        if (title.Nullify() == null)
        {
            return null;
        }
        return UnwantedAlbumTitleTextRegex.Replace(title!, string.Empty).Trim();
    }    

    public static bool SongHasUnwantedText(string? albumTitle, string? songTitle, int? songNumber)
    {
        if (string.IsNullOrWhiteSpace(songTitle))
        {
            return true;
        }

        if (StringHasFeaturingFragments(songTitle))
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
                if (string.Equals(songTitle.Trim(), (songNumber ?? 0).ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return Regex.IsMatch(songTitle, $@"^({Regex.Escape(albumTitle ?? string.Empty)}\s*.*\s*)?([0-9]*{songNumber}\s)");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SongHasUnwantedText For AlbumTitle [{AlbumTitle}] for SongTitle [{SongTitle}]", albumTitle, songTitle);
        }

        return false;
    }

    [GeneratedRegex(@"\s+with\s+|\s*;\s*|\s*(&|ft(\.)*|feat)\s*|\s+x\s+|\s*\,\s*", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ReplaceSongArtistSeparatorsRegex();
}
