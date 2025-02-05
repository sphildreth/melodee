using System.Text.RegularExpressions;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.MetaData.Directory.Nfo.Handlers;
using Melodee.Common.Plugins.MetaData.Directory.Nfo.Handlers.Jellyfin;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Common.Plugins.MetaData.Directory.Nfo;

/// <summary>
///     Processes NFO and gets tags and Songs for Album.
/// </summary>
public sealed partial class Nfo(ISerializer serializer, IAlbumValidator albumValidator, IMelodeeConfiguration configuration)
    : AlbumMetaDataBase(configuration), IDirectoryPlugin
{
    public const string HandlesExtension = "NFO";

    private readonly INfoHandler[] _nfoHandlers =
    [
        new PMediaHandler(),
        new JellyfinHandler()
    ];

    public override string Id => "35A33042-6E57-431C-AF94-F7F803F811C4";

    public override string DisplayName => nameof(Nfo);

    public override bool IsEnabled { get; set; }

    public override int SortOrder { get; } = 3;

    public async Task<OperationResult<int>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var nfoFiles = fileSystemDirectoryInfo.FileInfosForExtension(HandlesExtension).ToArray();

        if (nfoFiles.Length == 0)
        {
            return new OperationResult<int>("Skipping NFO. No NFO files found.")
            {
                Data = -1
            };
        }

        var processedFiles = 0;

        try
        {
            var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
            FileSystemDirectoryInfo? parentDirectory = null;
            if (dirInfo.Parent != null)
            {
                parentDirectory = new FileSystemDirectoryInfo
                {
                    Path = dirInfo.Parent.FullName,
                    Name = dirInfo.Parent.Name
                };
            }

            foreach (var nfoFile in nfoFiles)
            {
                using (Operation.At(LogEventLevel.Debug).Time("[{Plugin}] Processing [{FileName}]", DisplayName, nfoFile.Name))
                {
                    var nfoAlbum = await AlbumForNfoFileAsync(nfoFile, fileSystemDirectoryInfo, cancellationToken);
                    if (nfoAlbum == null)
                    {
                        continue;
                    }

                    var validationResult = albumValidator.ValidateAlbum(nfoAlbum);
                    nfoAlbum.ValidationMessages = validationResult.Data.Messages ?? [];
                    nfoAlbum.Status = validationResult.Data.AlbumStatus;
                    nfoAlbum.StatusReasons = validationResult.Data.AlbumStatusReasons;

                    var stagingAlbumDataName = Path.Combine(fileSystemDirectoryInfo.Path, nfoAlbum.ToMelodeeJsonName(MelodeeConfiguration));
                    if (File.Exists(stagingAlbumDataName))
                    {
                        var existingAlbum = await Album.DeserializeAndInitializeAlbumAsync(serializer, stagingAlbumDataName, cancellationToken).ConfigureAwait(false);
                        if (existingAlbum != null)
                        {
                            nfoAlbum = nfoAlbum.Merge(existingAlbum);
                        }
                    }

                    var serialized = serializer.Serialize(nfoAlbum);
                    await File.WriteAllTextAsync(stagingAlbumDataName, serialized, cancellationToken);
                    if (SafeParser.ToBoolean(Configuration[SettingRegistry.ProcessingDoDeleteOriginal]))
                    {
                        nfoFile.Delete();
                        Log.Information("[{Plugin}] Deleted NFO File [{FileName}]", DisplayName, nfoFile.Name);
                    }

                    Log.Debug("[{Plugin}] created [{StagingAlbumDataName}] Album [{Album}]", DisplayName, nfoAlbum.ToMelodeeJsonName(MelodeeConfiguration), nfoAlbum.ToString());
                    processedFiles++;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "[{Plugin}] processing directory [{DirName}]", DisplayName, fileSystemDirectoryInfo);
        }

        return new OperationResult<int>
        {
            Data = processedFiles
        };
    }

    public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        return fileSystemInfo.Extension(directoryInfo).DoStringsMatch(HandlesExtension);
    }

    private static (KeyValuePair<string, object?>, string?) ParseLine(string line, char splitChar)
    {
        var key = string.Empty;
        object? result = null;
        string? rawValue = null;

        var l = line.Nullify();
        if (!string.IsNullOrWhiteSpace(l))
        {
            try
            {
                var parts = l.Replace("[:", string.Empty).Replace(":]", ":").Split(splitChar);
                if (parts.Length > 1)
                {
                    key = parts[0].ToAlphanumericName(false, false).ToTitleCase(false).Nullify() ?? string.Empty;
                    result = ReplaceMultiplePeriodsRegex().Replace(parts[1].ToAlphanumericName(false, false).ToTitleCase(false)?.Nullify() ?? string.Empty, string.Empty);
                    rawValue = parts[1].Nullify();
                }
            }
            catch (Exception e)
            {
                Log.Warning($"Error attempting to parse [{line}] Error [{e}]", line, e);
            }
        }

        return (new KeyValuePair<string, object?>(key, result), rawValue);
    }

    private static bool IsLineForMatches(IEnumerable<string> matches, string line)
    {
        var l = line.ToAlphanumericName().Nullify();
        if (!string.IsNullOrWhiteSpace(l))
        {
            return matches.Any(match => l.Contains(match, StringComparison.OrdinalIgnoreCase));
        }

        return false;
    } // ReSharper disable StringLiteralTypo
    private static bool IsLineForAlbumArtist(string line)
    {
        return IsLineForMatches(["artist", "albumartist"], line);
    }

    private static bool IsLineForAlbumDate(string line)
    {
        return IsLineForMatches(["retaildate", "reldate", "ripdate"], line);
    }

    private static bool IsLineForAlbumTitle(string line)
    {
        return IsLineForMatches(["title"], line);
    }

    private static bool IsLineForSongTotal(string line)
    {
        return IsLineForMatches(["Songs"], line);
    }

    private static bool IsLineForLength(string line)
    {
        return IsLineForMatches(["length", "runtime"], line);
    }

    private static bool IsLineForPublisher(string line)
    {
        return IsLineForMatches(["label", "publisher"], line);
    }

    // ReSharper enable StringLiteralTypo

    private static readonly string[] _lineIgnores =
    [
        "length",
        "runtime"
    ];

    public static bool IsLineForSong(string line)
    {
        var l = line.Nullify();
        if (!string.IsNullOrWhiteSpace(l))
        {
            return IsLineForSongRegex().IsMatch(l) && _lineIgnores.All(x => !l.Contains(x, StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    public async Task<Album?> AlbumForNfoFileAsync(FileInfo fileInfo, FileSystemDirectoryInfo? parentDirectoryInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var nfoHandler in _nfoHandlers)
            {
                if (await nfoHandler.IsHandlerForNfoAsync(fileInfo, cancellationToken))
                {
                    return await nfoHandler.HandleNfoAsync(fileInfo, MelodeeConfiguration.GetValue<bool>(SettingRegistry.ProcessingDoDeleteOriginal), cancellationToken);
                }
            }

            var splitChar = ':';
            var albumTags = new List<MetaTag<object?>>();
            var songs = new List<Common.Models.Song>();

            var metaTagsToParseFromFile = new List<MetaTagIdentifier>
            {
                MetaTagIdentifier.AlbumArtist,
                MetaTagIdentifier.Encoder,
                MetaTagIdentifier.Genre
            };

            var mediaFilesForFolder = fileInfo.Directory?.ToDirectorySystemInfo().AllMediaTypeFileInfos().ToArray();

            foreach (var line in await File.ReadAllLinesAsync(fileInfo.FullName, cancellationToken))
            {
                if (!string.IsNullOrEmpty(line.ToAlphanumericName().Nullify()))
                {
                    var plr = ParseLine(line, splitChar);
                    var kp = plr.Item1;
                    var rawValue = plr.Item2;
                    if (IsLineForAlbumTitle(line))
                    {
                        albumTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Album,
                            Value = kp.Value
                        });
                        continue;
                    }

                    if (IsLineForAlbumArtist(line))
                    {
                        albumTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.AlbumArtist,
                            Value = kp.Value
                        });
                        continue;
                    }

                    if (IsLineForAlbumDate(line))
                    {
                        albumTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.RecordingYear,
                            Value = SafeParser.ToDateTime(rawValue?.OnlyAlphaNumeric() ?? string.Empty)?.Year
                        });
                        continue;
                    }

                    if (IsLineForSongTotal(line))
                    {
                        albumTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.SongTotal,
                            Value = SafeParser.ToNumber<int?>(rawValue?.OnlyAlphaNumeric() ?? string.Empty)
                        });
                        continue;
                    }

                    if (IsLineForLength(line))
                    {
                        albumTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Length,
                            Value = rawValue
                        });
                        continue;
                    }

                    if (IsLineForPublisher(line))
                    {
                        albumTags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Publisher,
                            Value = rawValue?.OnlyAlphaNumeric()
                        });
                        continue;
                    }

                    if (kp.Key.Nullify() != null)
                    {
                        foreach (var metaTagToParse in metaTagsToParseFromFile)
                        {
                            if (kp.Key.Equals(metaTagToParse.ToString(), StringComparison.OrdinalIgnoreCase) && kp.Value != null)
                            {
                                albumTags.Add(new MetaTag<object?>
                                {
                                    Identifier = metaTagToParse,
                                    Value = kp.Value
                                });
                                break;
                            }
                        }
                    }
                }

                if (IsLineForSong(line))
                {
                    var l = line.OnlyAlphaNumeric();
                    var songNumber = SafeParser.ToNumber<int>(l?.Substring(0, 2) ?? string.Empty);
                    var songDuration = l?.Substring(l.Length - 7).Trim() ?? string.Empty;
                    var songTitle = ReplaceMultiplePeriodsRegex().Replace(l?.Substring(3, l.Length - songDuration.Length - 4) ?? string.Empty, string.Empty).Trim();

                    var fileForSong = mediaFilesForFolder.FirstOrDefault(x => x.Name.ToNormalizedString()!.Contains(songTitle.ToNormalizedString()!, StringComparison.OrdinalIgnoreCase));
                    if (fileForSong == null)
                    {
                        Log.Warning("[{Plugin}] Could not find file for song [{SongTitle}] in [{DirName}]", DisplayName, songTitle, fileInfo.Directory?.FullName);
                        continue;
                    }

                    songs.Add(new Common.Models.Song
                    {
                        CrcHash = Crc32.Calculate(fileForSong),
                        Tags =
                        [
                            new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.TrackNumber,
                                Value = songNumber
                            },
                            new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.Title,
                                Value = songTitle
                            },
                            new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.Length,
                                Value = songDuration
                            }
                        ],
                        File = fileForSong.ToFileSystemInfo(),
                        SortOrder = songNumber
                    });
                }
            }

            if (songs.Any() && albumTags.All(x => x.Identifier != MetaTagIdentifier.DiscTotal))
            {
                var discTotalTags = songs
                    .Where(x => x.Tags != null)
                    .SelectMany(x => x.Tags!)
                    .Where(x => x.Identifier == MetaTagIdentifier.DiscTotal)
                    .ToArray();
                var maxSongDiscTotal = discTotalTags.Length != 0 ? discTotalTags.Max(x => SafeParser.ToNumber<short>(x.Value)) : 0;
                albumTags.Add(new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.DiscTotal,
                    Value = maxSongDiscTotal == 0 ? 1 : maxSongDiscTotal
                });
            }

            if (songs.Any())
            {
                var s = new List<Common.Models.Song>();
                foreach (var song in songs)
                {
                    if (song.AlbumArtist().Nullify() == null)
                    {
                        s.Add(song with
                        {
                            Tags = song.Tags?.Append(new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.AlbumArtist,
                                Value = albumTags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.AlbumArtist)?.Value
                            }).ToArray()
                        });
                    }
                }

                songs = s;
            }

            var artistName = albumTags.FirstOrDefault(x => x.Identifier is MetaTagIdentifier.Artist or MetaTagIdentifier.AlbumArtist)?.Value?.ToString();
            var albumName = albumTags.FirstOrDefault(x => x.Identifier is MetaTagIdentifier.Album)?.Value?.ToString();
            var result = new Album
            {
                AlbumType = albumName.TryToDetectAlbumType(),
                Artist = new Artist(
                    artistName ?? throw new Exception($"Invalid artist on {nameof(Nfo)}"),
                    artistName.ToNormalizedString() ?? artistName,
                    null),
                Directory = parentDirectoryInfo ?? fileInfo.Directory?.ToDirectorySystemInfo() ?? new FileSystemDirectoryInfo
                {
                    Path = fileInfo.Directory?.FullName ?? string.Empty,
                    Name = fileInfo.Directory?.Name ?? string.Empty
                },
                Files =
                [
                    new AlbumFile
                    {
                        AlbumFileType = AlbumFileType.MetaData,
                        ProcessedByPlugin = DisplayName,
                        FileSystemFileInfo = fileInfo.ToFileSystemInfo()
                    }
                ],
                ViaPlugins = [nameof(Nfo)],
                OriginalDirectory = new FileSystemDirectoryInfo
                {
                    ParentId = parentDirectoryInfo?.UniqueId ?? 0,
                    Path = fileInfo.Directory?.FullName ?? string.Empty,
                    Name = fileInfo.Directory?.Name ?? string.Empty
                },
                Images = songs.Where(x => x.Images != null).SelectMany(x => x.Images!).DistinctBy(x => x.CrcHash).ToArray(),
                Tags = albumTags.ToArray(),
                Songs = songs,
                SortOrder = 0
            };
            var validationResult = albumValidator.ValidateAlbum(result);
            result.ValidationMessages = validationResult.Data.Messages ?? [];
            result.Status = validationResult.Data.AlbumStatus;
            result.StatusReasons = validationResult.Data.AlbumStatusReasons;
            return result;
        }
        catch (Exception e)
        {
            Log.Error(e, "[{Plugin}] error processing NFO file", DisplayName);
        }

        return null;
    }

    //[GeneratedRegex(@"[0-9]+[a-z]+[0-9]{3,4}")]
    [GeneratedRegex(@".*([0-9]+:[0-9]+).*")]
    private static partial Regex IsLineForSongRegex();

    [GeneratedRegex(@"\.{2,}")]
    private static partial Regex ReplaceMultiplePeriodsRegex();
}
