using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Directory.Models;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Validation;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Plugins.MetaData.Directory;

public sealed class M3UPlaylist(ISerializer serializer, IEnumerable<ISongPlugin> songPlugins, IAlbumValidator albumValidator, IMelodeeConfiguration configuration) : AlbumMetaDataBase(configuration), IDirectoryPlugin
{
    public const string HandlesExtension = "M3U";

    public override string Id => "800EBFEF-4A9A-4DD8-8505-056D13535D45";

    public override string DisplayName => nameof(M3UPlaylist);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 2;

    public async Task<OperationResult<int>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var processedFiles = 0;

        try
        {
            var m3UFiles = fileSystemDirectoryInfo.FileInfosForExtension(HandlesExtension).ToArray();

            if (m3UFiles.Length == 0)
            {
                return new OperationResult<int>("Skipping validation. No M3U file for Album.")
                {
                    Data = -1
                };
            }

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

            var songPlugin = songPlugins.First();
            foreach (var m3UFile in m3UFiles)
            {
                using (Operation.At(LogEventLevel.Debug).Time("[{Plugin}] Processing [{FileName}]", DisplayName, m3UFile.Name))
                {
                    var models = await GetModelsFromM3UFile(m3UFile.FullName);

                    var songs = new List<Common.Models.Song>();
                    await Parallel.ForEachAsync(models, cancellationToken, async (model, tt) =>
                    {
                        var songResult = await songPlugin.ProcessFileAsync(fileSystemDirectoryInfo, model.FileSystemFileInfo, tt);
                        if (songResult.IsSuccess)
                        {
                            songs.Add(songResult.Data);
                        }
                    });
                    if (songs.Count > 0)
                    {
                        var firstSong = songs.OrderBy(x => x.SortOrder).First(x => x.Tags != null);
                        var songTotal = firstSong.SongTotalNumber();
                        if (songTotal < 1)
                        {
                            if (models.Length == songs.Count)
                            {
                                songTotal = models.Length;
                            }
                        }

                        var newAlbumTags = new List<MetaTag<object?>>
                        {
                            new() { Identifier = MetaTagIdentifier.Album, Value = firstSong.AlbumTitle(), SortOrder = 1 },
                            new() { Identifier = MetaTagIdentifier.AlbumArtist, Value = firstSong.AlbumArtist(), SortOrder = 2 },
                            new() { Identifier = MetaTagIdentifier.DiscNumber, Value = firstSong.MediaNumber(), SortOrder = 4 },
                            new() { Identifier = MetaTagIdentifier.OrigAlbumYear, Value = firstSong.AlbumYear(), SortOrder = 100 },
                            new() { Identifier = MetaTagIdentifier.SongTotal, Value = songTotal, SortOrder = 101 }
                        };
                        var genres = songs
                            .SelectMany(x => x.Tags ?? Array.Empty<MetaTag<object?>>())
                            .Where(x => x.Identifier == MetaTagIdentifier.Genre)
                            .ToArray();
                        if (genres.Length != 0)
                        {
                            newAlbumTags.AddRange(genres
                                .GroupBy(x => x.Value)
                                .Select((genre, i) => new MetaTag<object?>
                                {
                                    Identifier = MetaTagIdentifier.Genre,
                                    Value = genre.Key,
                                    SortOrder = 5 + i
                                }));
                        }

                        var m3UAlbum = new Album
                        {
                            Artist = new Artist(
                                firstSong.AlbumArtist() ?? throw new Exception($"Invalid artist on { nameof(CueSheet) }"),
                                firstSong.AlbumArtist().ToNormalizedString(),
                                null),                           
                            Directory = fileSystemDirectoryInfo,
                            Files = new[]
                            {
                                new AlbumFile
                                {
                                    AlbumFileType = AlbumFileType.MetaData,
                                    ProcessedByPlugin = DisplayName,
                                    FileSystemFileInfo = m3UFile.ToFileSystemInfo()
                                }
                            },
                            OriginalDirectory = new FileSystemDirectoryInfo
                            {
                                ParentId = parentDirectory?.UniqueId ?? 0,
                                Path = fileSystemDirectoryInfo.Path,
                                Name = fileSystemDirectoryInfo.Name,
                                TotalItemsFound = songs.Count,
                                MusicFilesFound = songs.Count,
                                MusicMetaDataFilesFound = 1
                            },
                            Images = songs.Where(x => x.Images != null).SelectMany(x => x.Images!).DistinctBy(x => x.CrcHash).ToArray(),
                            Tags = newAlbumTags,
                            Songs = songs.OrderBy(x => x.SortOrder).ToArray(),
                            ViaPlugins = new[] { songPlugin.DisplayName, DisplayName }
                        };
                        var isValidCheck = m3UAlbum.IsValid(Configuration);
                        if (!isValidCheck.Item1)
                        {
                            m3UAlbum.ValidationMessages = m3UAlbum.ValidationMessages.Append(new ValidationResultMessage
                            {
                                Message = isValidCheck.Item2 ?? "Album failed validation.",
                                Severity = ValidationResultMessageSeverity.Critical
                            });
                        }

                        m3UAlbum.Status = isValidCheck.Item1 ? AlbumStatus.Ok : AlbumStatus.Invalid;
                        var stagingAlbumDataName = Path.Combine(fileSystemDirectoryInfo.Path, m3UAlbum.ToMelodeeJsonName(MelodeeConfiguration));
                        if (File.Exists(stagingAlbumDataName))
                        {
                            var existingAlbum = serializer.Deserialize<Album?>(await File.ReadAllTextAsync(stagingAlbumDataName, cancellationToken));
                            if (existingAlbum != null)
                            {
                                m3UAlbum = m3UAlbum.Merge(existingAlbum);
                            }
                        }

                        var serialized = serializer.Serialize(m3UAlbum);
                        await File.WriteAllTextAsync(stagingAlbumDataName, serialized, cancellationToken);
                        if (SafeParser.ToBoolean(Configuration[SettingRegistry.ProcessingDoDeleteOriginal]))
                        {
                            m3UFile.Delete();
                            Log.Information("Deleted M3U File [{FileName}]", m3UFile.Name);
                        }

                        Log.Debug("[{Plugin}] created [{StagingAlbumDataName}] Status [{Status}]", DisplayName, m3UAlbum.ToMelodeeJsonName(MelodeeConfiguration), m3UAlbum.Status.ToString());
                        processedFiles++;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "[{Name}] processing directory [{DirName}]", DisplayName, fileSystemDirectoryInfo);
            StopProcessing = true;
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

    private static async Task<M3ULine[]> GetModelsFromM3UFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return [];
        }

        var result = new List<M3ULine>();
        try
        {
            foreach (var line in await File.ReadAllLinesAsync(filePath))
            {
                var model = ModelFromM3ULine(filePath, line);
                if (model != null)
                {
                    result.Add(model);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "FilePath [{FilePath}]", filePath);
        }

        return result.ToArray();
    }

    public static M3ULine? ModelFromM3ULine(string filePath, string? lineFromFile)
    {
        if (string.IsNullOrWhiteSpace(lineFromFile))
        {
            return null;
        }

        try
        {
            var directoryInfo = filePath.ToDirectoryInfo();
            var parts = lineFromFile.Split('-');
            if (parts.Length >= 3)
            {
                var albumArtist = parts[1];
                var songTitle = parts[2];

                var fileSystemInfoFile = new FileSystemFileInfo
                {
                    Name = lineFromFile,
                    Size = 0
                };

                string? dirName = null;
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    var fi = new FileInfo(filePath);
                    if (fi.Directory?.Exists ?? false)
                    {
                        dirName = fi.DirectoryName!;
                    }

                    fileSystemInfoFile = new FileInfo(Path.Combine(dirName ?? string.Empty, lineFromFile)).ToFileSystemInfo();
                }

                return new M3ULine
                {
                    IsValid = !string.IsNullOrWhiteSpace(filePath) && fileSystemInfoFile.Exists(directoryInfo),
                    FileSystemFileInfo = fileSystemInfoFile,
                    AlbumArist = albumArtist.Replace("_", " ").CleanString(true),
                    SongNumber = SafeParser.ToNumber<int>(parts[0]),
                    SongTitle = songTitle.Replace("_", " ").RemoveFileExtension()!.CleanString() ?? string.Empty
                };
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "lineFromFile [{LineFromFile}]", lineFromFile);
        }

        return null;
    }
}
