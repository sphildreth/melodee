using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Plugins.MetaData.Directory.Nfo.Handlers.Jellyfin;

/// <summary>
///     Handles NFO files created by Jellyfin
///     <remarks>
///         see
///         https://github.com/jellyfin/jellyfin/blob/17bbe4a2cd7f7c7cdcd4a2ce06e9e076c616d7ae/MediaBrowser.XbmcMetadata/Savers/AlbumNfoSaver.cs
///     </remarks>
/// </summary>
public sealed class JellyfinHandler : INfoHandler
{
    public async Task<bool> IsHandlerForNfoAsync(FileInfo fileInfo, CancellationToken cancellationToken = default)
    {
        var fileContents = await File.ReadAllTextAsync(fileInfo.FullName, cancellationToken).ConfigureAwait(false);
        if (fileContents.Length > 0)
        {
            try
            {
                var deserializer = new JellyfinXmlDeserializer<Models.Jellyfin.Album>();
                return deserializer.Deserialize(fileContents).Title != null;
            }
            catch
            {
                // Likely invalid XML
            }
        }

        return false;
    }

    public async Task<Album?> HandleNfoAsync(FileInfo fileInfo, bool doDeleteOriginal,
        CancellationToken cancellationToken = default)
    {
        var isForJellyfin = await IsHandlerForNfoAsync(fileInfo, cancellationToken);
        if (isForJellyfin && fileInfo.DirectoryName != null)
        {
            var fileContents = await File.ReadAllTextAsync(fileInfo.FullName, cancellationToken).ConfigureAwait(false);
            var deserializer = new JellyfinXmlDeserializer<Models.Jellyfin.Album>();
            var jfAlbum = deserializer.Deserialize(fileContents);
            var albumTags = new List<MetaTag<object?>>
            {
                new()
                {
                    Identifier = MetaTagIdentifier.Album,
                    Value = jfAlbum.Title
                },
                new()
                {
                    Identifier = MetaTagIdentifier.OrigAlbumYear,
                    Value = jfAlbum.Year ?? jfAlbum.ReleaseDate?.Year
                },
                new()
                {
                    Identifier = MetaTagIdentifier.GeneralDescription,
                    Value = jfAlbum.Outline
                },
                new()
                {
                    Identifier = MetaTagIdentifier.SongTotal,
                    Value = jfAlbum.Track?.Count ?? 0
                },
                new()
                {
                    Identifier = MetaTagIdentifier.Genre,
                    Value = jfAlbum.Genre
                },
                new()
                {
                    Identifier = MetaTagIdentifier.MusicBrainzId,
                    Value = jfAlbum.MusicBrainzAlbumId
                }
            };

            var albumDirectory = new DirectoryInfo(fileInfo.DirectoryName).ToDirectorySystemInfo();

            var songs = new List<Common.Models.Song>();
            var mediaTypeFilesInDirectory = albumDirectory.AllMediaTypeFileInfos().ToArray();
            foreach (var track in jfAlbum.Track ?? [])
            {
                var trackFileNameWithoutExtension =
                    $"{track.Position.ToStringPadLeft(2)} {track.Title}".ToNormalizedString() ?? string.Empty;
                var mediaTrackForTrack = mediaTypeFilesInDirectory.FirstOrDefault(x =>
                    x.Name.ToNormalizedString()!.StartsWith(trackFileNameWithoutExtension));
                if (mediaTrackForTrack != null)
                {
                    var songTags = new List<MetaTag<object?>>
                    {
                        new()
                        {
                            Identifier = MetaTagIdentifier.TrackNumber,
                            Value = SafeParser.ToNumber<short>(track.Position)
                        },
                        new()
                        {
                            Identifier = MetaTagIdentifier.Title,
                            Value = track.Title
                        }
                    };
                    var mediaAudios = new List<MediaAudio<object?>>
                    {
                        new()
                        {
                            Identifier = MediaAudioIdentifier.DurationMs,
                            Value = SafeParser.ToNumber<int>(track.Duration?.Replace(":", string.Empty)) * 1000
                        }
                    };
                    songs.Add(new Common.Models.Song
                    {
                        CrcHash = Crc32.Calculate(mediaTrackForTrack),
                        File = mediaTrackForTrack.ToFileSystemInfo(),
                        Tags = songTags,
                        MediaAudios = mediaAudios,
                        SortOrder = SafeParser.ToNumber<short>(track.Position)
                    });
                }
            }

            var images = new List<ImageInfo>();
            foreach (var art in jfAlbum.Art?.Where(x => x.Poster != null) ?? [])
            {
                var artFileInfo = new FileInfo(art.Poster!);
                if (!artFileInfo.Exists)
                {
                    artFileInfo = new FileInfo(Path.Combine(albumDirectory.FullName(), artFileInfo.Name));
                }

                if (artFileInfo.Exists)
                {
                    images.Add(new ImageInfo
                    {
                        CrcHash = Crc32.Calculate(artFileInfo),
                        FileInfo = artFileInfo.ToFileSystemInfo()
                    });
                }
            }

            var artistName = jfAlbum.AlbumArtist ??
                             jfAlbum.Actor?.FirstOrDefault(x => x.Type == "AlbumArtist")?.Name ??
                             throw new Exception($"Invalid artist on [{fileInfo.FullName}]");

            var result = new Album
            {
                Artist = new Artist(artistName,
                    artistName.ToNormalizedString() ?? artistName,
                    artistName.ToTitleCase() ?? artistName)
                {
                    MusicBrainzId = jfAlbum.MusicBrainzAlbumArtistId == null
                        ? null
                        : Guid.Parse(jfAlbum.MusicBrainzAlbumArtistId)
                },
                Files =
                [
                    new AlbumFile
                    {
                        AlbumFileType = AlbumFileType.MetaData,
                        ProcessedByPlugin = nameof(JellyfinHandler),
                        FileSystemFileInfo = fileInfo.ToFileSystemInfo()
                    }
                ],
                Images = images,
                Tags = albumTags.ToArray(),
                Songs = songs,
                SortOrder = 0,
                ViaPlugins = [nameof(JellyfinHandler)],
                OriginalDirectory = albumDirectory,
                Directory = albumDirectory
            };

            if (doDeleteOriginal)
            {
                fileInfo.Delete();
            }

            return result;
        }

        return null;
    }
}
