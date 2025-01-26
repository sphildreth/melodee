using System.Text;
using System.Text.RegularExpressions;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.Validation;
using Melodee.Common.Plugins.Validation.Models;
using Melodee.Common.Utility;
using Serilog;

namespace Melodee.Common.Plugins.Validation;

public sealed partial class AlbumValidator(IMelodeeConfiguration configuration) : IAlbumValidator
{
    private static readonly Regex UnwantedAlbumTitleTextRegex = new(@"(\s*(-\s)*((CD[_\-#\s]*[0-9]*)))|(\s[\[\(]*(lp|ep|bonus|Album|re(\-*)issue|re(\-*)master|re(\-*)mastered|anniversary|single|cd|disc|deluxe|digipak|digipack|vinyl|japan(ese)*|asian|remastered|limited|ltd|expanded|(re)*\-*edition|web|\(320\)|\(*compilation\)*)+(]|\)*))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex UnwantedSongTitleTextRegex = new(@"(\s{2,}|(\s\(prod\s))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static readonly Regex HasFeatureFragmentsRegex = new(@"(\s[\(\[]*ft[\s\.]|\s*[\(\[]*with\s+|\s*[\(\[]*feat[\s\.]|[\(\[]*(featuring))+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly Dictionary<string, object?> _configuration = configuration.Configuration;
    private readonly List<ValidationResultMessage> _validationMessages = [];

    private AlbumNeedsAttentionReasons _albumNeedsAttentionReasons = AlbumNeedsAttentionReasons.NotSet;

    public OperationResult<AlbumValidationResult> ValidateAlbum(Album? album)
    {
        if (album == null)
        {
            return new OperationResult<AlbumValidationResult>(["Unable to validate album."])
            {
                Data = new AlbumValidationResult(AlbumStatus.Invalid, AlbumNeedsAttentionReasons.AlbumCannotBeLoaded)
            };
        }

        _validationMessages.Clear();
        _albumNeedsAttentionReasons = AlbumNeedsAttentionReasons.NotSet;

        if (!album.Songs?.Any() ?? false)
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasNoSongs;
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album has no songs.",
                Severity = ValidationResultMessageSeverity.Critical
            });
        }

        if (album.Tags?.Count() == 0)
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasNoTags;
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album has no tags.",
                Severity = ValidationResultMessageSeverity.Critical
            });
        }

        if (!album.Artist.IsValid())
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasInvalidArtists;
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album artist is invalid.",
                Severity = ValidationResultMessageSeverity.Critical
            });
        }

        if (album.Songs?.Any(x => !x.IsValid(_configuration)) ?? false)
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasInvalidSongs;
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album has invalid songs.",
                Severity = ValidationResultMessageSeverity.Critical
            });
        }

        if (!album.HasValidAlbumYear(_configuration))
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasInvalidYear;
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album has invalid year.",
                Severity = ValidationResultMessageSeverity.Critical
            });
        }

        var albumTitle = album.AlbumTitle().Nullify();
        if (albumTitle == null)
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.TitleIsInvalid;
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album title is invalid.",
                Severity = ValidationResultMessageSeverity.Critical
            });
        }

        try
        {
            AreAllSongNumbersValid(album);
            AreSongsUniquelyNumbered(album);
            AreMediaNumbersValid(album);
            DoAllSongsHaveSameAlbumArtist(album);
            AllSongTitlesDoNotHaveUnwantedText(album);
            AlbumArtistDoesNotHaveUnwantedText(album);
            AlbumTitleDoesNotHaveUnwantedText(album);
            DoMediaTotalMatchMediaNumbers(album);
            DoAllSongsHaveMediaNumberSet(album);
            DoesSongTotalMatchSongCount(album);
            DoesAlbumHaveCoverImage(album);
            ArtistHasSearchEngineResult(album.Artist);
            AlbumIsStudioTypeAlbum(album);
            AllSongsHaveAnArtist(album);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        var albumStatus = _validationMessages.All(x => x.Severity != ValidationResultMessageSeverity.Critical)
            ? AlbumStatus.Ok
            : AlbumStatus.Invalid;
        return new OperationResult<AlbumValidationResult>
        {
            Data = new AlbumValidationResult(albumStatus, _albumNeedsAttentionReasons)
            {
                IsValid = albumStatus == AlbumStatus.Ok,
                Messages = _validationMessages
            }
        };
    }

    private void AlbumIsStudioTypeAlbum(Album album)
    {
        if (!album.IsStudioTypeAlbum())
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album is not studio type, will need manual validation.",
                Severity = ValidationResultMessageSeverity.Critical
            });
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.IsNotStudioTypeAlbum;
        }
    }

    private void AllSongsHaveAnArtist(Album album)
    {
        var anySongsWithoutArtists = album.Songs?.Any(x => x.AlbumArtist().Nullify() == null) ?? false;
        if (anySongsWithoutArtists)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album has songs with no artist, will need manual validation.",
                Severity = ValidationResultMessageSeverity.Critical
            });
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasSongsWithoutArtists;
        }
    }

    private void ArtistHasSearchEngineResult(Artist albumArtist)
    {
        if (albumArtist.SearchEngineResultUniqueId == null)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album Artist is unknown, will need manual validation.",
                Severity = ValidationResultMessageSeverity.Critical
            });
            if (albumArtist.Name.Nullify() == null && _albumNeedsAttentionReasons.HasFlag(AlbumNeedsAttentionReasons.ArtistIsNotSet) == false)
            {
                _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasUnknownArtist;
            }
            else if (!_albumNeedsAttentionReasons.HasFlag(AlbumNeedsAttentionReasons.ArtistIsNotSet))
            {
                _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.ArtistIsNotSet;
            }
        }
    }

    private void DoesAlbumHaveCoverImage(Album album)
    {
        var result = album.CoverImage() != null;
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album does not have cover image.",
                Severity = ValidationResultMessageSeverity.Critical
            });
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasNoImages;
        }
    }

    private void DoesSongTotalMatchSongCount(Album album)
    {
        var songs = album.Songs?.ToArray() ?? [];
        var songCount = songs.Length;
        var songTotal = album.SongTotalValue();
        var result = songCount == songTotal;
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album Song total value does not match Song count.",
                Severity = ValidationResultMessageSeverity.Critical
            });
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.SongTotalDoesntMatchSongCount;
        }
    }

    private void DoAllSongsHaveMediaNumberSet(Album album)
    {
        var songsGroupedByMediaNumber = (album.Songs?.GroupBy(x => x.MediaNumber()) ?? []).ToArray();
        if (!songsGroupedByMediaNumber.Select(x => SafeParser.ToNumber<int>(x.Key)).Order().ToArray().AreNumbersSequential())
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.SongsAreNotSequentiallyNumbered;
        }

        if (songsGroupedByMediaNumber.Any(mediaGroups => !mediaGroups.Select(x => x.SongNumber()).Order().ToArray().AreNumbersSequential()))
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.MediaNumbersAreInvalid;
        }

        var songs = album.Songs?.ToArray() ?? [];
        foreach (var song in songs)
        {
            if (song.MediaNumber() < 1)
            {
                _validationMessages.Add(new ValidationResultMessage
                {
                    Message = $"Song [{song.DisplaySummary}] has invalid media number.",
                    Severity = ValidationResultMessageSeverity.Critical
                });
                _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasSongsWithoutMediaNumberSet;
            }
        }
    }

    private void DoMediaTotalMatchMediaNumbers(Album album)
    {
        var songs = album.Songs?.ToArray() ?? [];
        var mediaNumbers = songs.Select(x => x.MediaNumber()).Distinct().ToArray();
        var albumMediaTotal = album.MediaCountValue();
        var result = mediaNumbers.All(mediaNumber => mediaNumber <= albumMediaTotal);
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = $"Album media total [{albumMediaTotal}] does not match Song medias [{string.Join(',', mediaNumbers)}].",
                Severity = ValidationResultMessageSeverity.Critical
            });
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.MediaTotalNumberDoesntMatchMediaFound;
        }
    }

    private void AreSongsUniquelyNumbered(Album album)
    {
        var songs = album.Songs?.ToArray() ?? [];
        if (songs.Length == 0)
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasNoSongs;
            return;
        }

        var result = true;
        foreach (var mediaSongs in songs.GroupBy(x => x.MediaNumber()))
        {
            var songNumbers = mediaSongs.GroupBy(x => x.SongNumber());
            if (songNumbers.Any(group => group.Count() > 1))
            {
                result = false;
            }
        }

        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album has Songs with invalid Song number.",
                Severity = ValidationResultMessageSeverity.Critical
            });
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.SongsAreNotUniquelyNumbered;
        }
    }

    /// <summary>
    ///     Check if all the Songs have the same Album Artist. This is an issue if not a VA type Album.
    /// </summary>
    private void DoAllSongsHaveSameAlbumArtist(Album album)
    {
        var result = true;
        var songs = album.Songs?.ToArray() ?? [];
        if (songs.Length == 0)
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasNoSongs;
            result = false;
        }

        var albumArtist = album.Artist.Name;
        if (string.IsNullOrWhiteSpace(albumArtist))
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.ArtistIsNotSet;
            result = false;
        }

        if (result)
        {
            var songsGroupedByArtist = songs.GroupBy(x => x.AlbumArtist()).ToArray();
            if (songsGroupedByArtist.Length > 1)
            {
                result = songsGroupedByArtist.First().Key.Nullify() == null ||
                         (string.Equals(songsGroupedByArtist.First().Key, albumArtist) && songsGroupedByArtist.Length == 1);
            }
        }

        if (!result && !album.IsVariousArtistTypeAlbum())
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Songs do not all have the same album artist.",
                Severity = ValidationResultMessageSeverity.Critical
            });
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasMultipleArtistsButNotMultipleAristAlbumType;
        }
    }

    private void AlbumArtistDoesNotHaveUnwantedText(Album album)
    {
        var result = !StringHasFeaturingFragments(album.Artist.Name);
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album artist has unwanted text.",
                Severity = ValidationResultMessageSeverity.Undesired
            });
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.ArtistNameHasUnwantedText;
        }
    }

    private void AlbumTitleDoesNotHaveUnwantedText(Album album)
    {
        var result = !StringHasFeaturingFragments(album.AlbumTitle());
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album title has unwanted text.",
                Severity = ValidationResultMessageSeverity.Undesired
            });
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.TitleHasUnwantedText;
        }
    }

    private void AreMediaNumbersValid(Album album)
    {
        var result = true;
        var songs = album.Songs?.ToArray() ?? [];
        if (songs.Length == 0)
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasNoSongs;
            result = false;
        }

        var mediaNumbers = songs.Select(x => x.MediaNumber()).Distinct().Order().ToArray();
        if (mediaNumbers.Length != 0 && mediaNumbers.All(x => x > 0))
        {
            var maxMediaNumber = SafeParser.ToNumber<int>(_configuration[SettingRegistry.ValidationMaximumMediaNumber]);
            var minMediaNumber = 1;
            if (mediaNumbers.Any(x => x > maxMediaNumber) ||
                mediaNumbers.Any(x => x < minMediaNumber))
            {
                result = false;
            }
            else
            {
                result = Enumerable.Range(0, mediaNumbers.Length).All(i => mediaNumbers[i] == mediaNumbers[0] + i);
            }
        }

        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album media numbers are invalid.",
                Severity = ValidationResultMessageSeverity.Critical
            });
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.MediaNumbersAreInvalid;
        }
    }

    private void AllSongTitlesDoNotHaveUnwantedText(Album album)
    {
        var result = true;
        var songs = album.Songs?.ToArray() ?? [];
        if (songs.Length == 0)
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasNoSongs;
            result = false;
        }

        var messageResult = new StringBuilder("Some Songs have unwanted text: ");
        if (result)
        {
            foreach (var song in songs)
            {
                if (SongHasUnwantedText(album.AlbumTitle(), song.Title(), song.SongNumber()))
                {
                    messageResult.Append($"[{song}]");
                    result = false;
                }
            }
        }

        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = messageResult.ToString(),
                Severity = ValidationResultMessageSeverity.Undesired
            });
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasSongsWithUnwantedText;
        }
    }


    private void AreAllSongNumbersValid(Album album)
    {
        var result = true;
        var songs = album.Songs?.ToArray() ?? [];
        if (songs.Length == 0)
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasNoSongs;
            result = false;
        }

        var songNumbers = songs.Select(x => x.SongNumber()).Distinct().ToArray();
        if (songNumbers.Length == 0)
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.SongsAreNotUniquelyNumbered;
            result = false;
        }

        if (songNumbers.Contains(0))
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasSongsWithInvalidNumber;
            result = false;
        }

        var firstSongNumber = songNumbers.Length > 0 ? songNumbers.First() : 0;
        if (firstSongNumber != 1)
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasInvalidFirstSongNumber;
            result = false;
        }

        var maximumSongNumber = SafeParser.ToNumber<int>(_configuration[SettingRegistry.ValidationMaximumSongNumber]);
        if (songNumbers.Any(x => x > maximumSongNumber))
        {
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.HasSongsWithNumberGreaterThanMaximumAllowed;
            result = false;
        }

        result = result && Enumerable.Range(0, songNumbers.Length).All(i => songNumbers[i] == songNumbers[0] + i);
        if (!result)
        {
            _validationMessages.Add(new ValidationResultMessage
            {
                Message = "Album Song numbers are invalid.",
                Severity = ValidationResultMessageSeverity.Critical
            });
            _albumNeedsAttentionReasons |= AlbumNeedsAttentionReasons.SongsAreNotSequentiallyNumbered;
        }
    }

    public static bool StringHasFeaturingFragments(string? input)
    {
        return !string.IsNullOrWhiteSpace(input) && HasFeatureFragmentsRegex.IsMatch(input);
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

    public static string? RemoveUnwantedTextFromSongTitle(string? title)
    {
        if (title?.CleanString().Nullify() == null)
        {
            return null;
        }

        return UnwantedSongTitleTextRegex.Replace(title.CleanString(doTitleCase: false)!, string.Empty).Trim();
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
