using Melodee.Common.Data.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Utility;

namespace Melodee.Common.Data.Models.Extensions;

public static class AlbumExtensions
{
    public static KeyValue ToKeyValue(this Album album)
    {
        return new KeyValue(album.ApiKey.ToString(), album.Name.ToNormalizedString() ?? album.Name);
    }

    public static string ToCoverArtId(this Album album)
    {
        return album.ToApiKey();
    }

    public static string ToApiKey(this Album album)
    {
        return $"album{OpenSubsonicServer.ApiIdSeparator}{album.ApiKey}";
    }

    public static int DistinctContributorCount(this Album album)
    {
        return album.Contributors.Any() ? album.Contributors.DistinctBy(x => x.ContributorName).Count() : 0;
    }

    public static bool DoShowExternalLinks(this Album album)
    {
        return album.AmgId.Nullify() != null ||
               album.DiscogsId.Nullify() != null ||
               album.ItunesId.Nullify() != null ||
               album.LastFmId.Nullify() != null ||
               album.MusicBrainzId != null ||
               album.SpotifyId.Nullify() != null ||
               album.WikiDataId.Nullify() != null;
    }

    /// <summary>
    ///     This is primarily to use the Album extensions when adding/editing Albums, not a fully populated model.
    /// </summary>
    public static Common.Models.Album ToMelodeeAlbumModel(this Album album)
    {
        var albumTags = new List<MetaTag<object?>>
        {
            new()
            {
                Identifier = MetaTagIdentifier.Album,
                Value = album.Name
            },
            new()
            {
                Identifier = MetaTagIdentifier.RecordingYear,
                Value = album.ReleaseDate.Year
            }
        };
        return new Common.Models.Album
        {
            Artist = album.Artist.ToMelodeeArtistModel(),
            Tags = albumTags,
            ViaPlugins = [],
            OriginalDirectory = new FileSystemDirectoryInfo { Path = album.Directory, Name = album.Name },
            Directory = new FileSystemDirectoryInfo { Path = album.Directory, Name = album.Name }
        };
    }

    public static AlbumDataInfo ToAlbumDataInfo(this Album album)
    {
        return new AlbumDataInfo(album.Id,
            album.ApiKey,
            album.IsLocked,
            album.Name,
            album.NameNormalized,
            album.AlternateNames,
            album.Artist.ApiKey,
            album.Artist.Name,
            album.SongCount ?? 0,
            album.Duration,
            album.CreatedAt,
            album.Tags,
            album.ReleaseDate,
            album.AlbumStatus
        );
    }

    public static RecordLabel[]? RecordLabels(this Album album)
    {
        if (album.Contributors.Any())
        {
            var publisher = album.Contributors.Where(x => x.ContributorTypeValue == ContributorType.Publisher)
                .ToArray();
            if (publisher.Length > 0)
            {
                return publisher.Select(x =>
                        new RecordLabel(x.ContributorName ??
                                        throw new Exception(
                                            "Album contributor of Publisher cannot have a null ContributorName")))
                    .ToArray();
            }
        }

        return null;
    }

    public static ArtistID3[] ContributingArtists(this Album album)
    {
        var result = new List<ArtistID3>();
        var songsWithContributors = album.Songs.Where(x => x.Contributors.Count != 0).ToArray();
        if (songsWithContributors.Length > 0)
        {
            foreach (var song in songsWithContributors)
            {
                foreach (var artistContributor in song.Contributors.Where(x =>
                             x.ContributorTypeValue == ContributorType.Performer))
                {
                    if (artistContributor.Artist != null)
                    {
                        result.Add(artistContributor.Artist!.ToApiArtistID3());
                    }
                    else
                    {
                        var id =
                            $"contributor{OpenSubsonicServer.ApiIdSeparator}{artistContributor.ContributorName.ToNormalizedString()}";
                        if (artistContributor.ContributorName != null)
                        {
                            result.Add(new ArtistID3(
                                id,
                                artistContributor.ContributorName,
                                id,
                                0,
                                0,
                                null,
                                null,
                                null,
                                null,
                                null
                            ));
                        }
                    }
                }
            }
        }

        return result.ToArray();
    }

    public static AlbumID3 ToArtistID3(this Album album, UserAlbum? userAlbum, NowPlayingInfo? nowPlayingInfo)
    {
        return new AlbumID3
        {
            Id = album.ToApiKey(),
            Name = album.Name,
            Artist = album.Artist.Name,
            ArtistId = album.Artist.ToApiKey(),
            CoverArt = album.ToCoverArtId(),
            SongCount = album.SongCount ?? 0,
            Duration = album.Duration.ToSeconds(),
            PlayCount = album.PlayedCount,
            CreatedRaw = album.CreatedAt,
            Starred = userAlbum?.StarredAt?.ToString(),
            Year = album.ReleaseDate.Year,
            Genres = album.Genres
        };
    }

    public static Child ToApiChild(this Album album, UserAlbum? userAlbum, NowPlayingInfo? nowPlayingInfo = null)
    {
        Contributor? albumArtist = null;

        return new Child(album.ToApiKey(),
            album.Artist.ToApiKey(),
            true,
            album.Name,
            album.Name,
            albumArtist == null ? album.Artist.Name : albumArtist.Artist!.Name,
            null,
            album.ReleaseDate.Year,
            album.ToCoverArtId(),
            null,
            null,
            null,
            userAlbum?.IsStarred ?? false ? userAlbum.LastUpdatedAt.ToString() : null,
            album.Duration.ToSeconds(),
            null,
            null,
            null,
            null,
            null,
            album.PlayedCount,
            album.LastPlayedAt?.ToString(),
            null,
            album.CreatedAt.ToString(),
            album.ToApiKey(),
            album.Artist.ToApiKey(),
            "music",
            "album",
            false,
            null,
            null,
            album.SortName,
            album.MusicBrainzId?.ToString(),
            [], //TODO
            [], //TODO
            album.Artist.Name,
            [], //TODO
            album.Artist.Name,
            [], //TODO
            null, //TODO
            [], //TODO
            null, //TODO
            SafeParser.ToNumber<int>(album.CalculatedRating),
            userAlbum?.Rating,
            nowPlayingInfo?.User.UserName,
            nowPlayingInfo?.Scrobble.MinutesAgo,
            0,
            nowPlayingInfo?.Scrobble.PlayerName
        );
    }
    
    public static FileSystemDirectoryInfo ToFileSystemDirectoryInfo(this Album album, string? libraryPath = null)
    {
        return new FileSystemDirectoryInfo
        {
            Path = Path.Combine(libraryPath ?? album.Artist.Library.Path, album.Artist.Directory),
            Name = album.Directory
        };
    }    
    
}
