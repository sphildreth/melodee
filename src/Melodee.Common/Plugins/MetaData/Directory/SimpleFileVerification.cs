using System.Diagnostics;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.MetaData.Directory.Models;
using Melodee.Common.Plugins.MetaData.Song;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Common.Plugins.MetaData.Directory;

/// <summary>
///     Processes Simple Verification Files (SFV) and gets files (Songs) and files CRC for Album.
/// </summary>
public sealed class SimpleFileVerification(ISerializer serializer, IEnumerable<ISongPlugin> songPlugins, IAlbumValidator albumValidator, IMelodeeConfiguration configuration) : AlbumMetaDataBase(configuration), IDirectoryPlugin
{
    public const string HandlesExtension = "SFV";
    private readonly IAlbumValidator _albumValidator = albumValidator;

    private readonly IEnumerable<ISongPlugin> _songPlugins = songPlugins;
    public override string Id => "6C253D42-F176-4A58-A895-C54BEB1F8A5C";

    public override string DisplayName => nameof(SimpleFileVerification);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 1;

    public async Task<OperationResult<int>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var resultType = OperationResponseType.Error;
        var processedFiles = 0;

        try
        {
            var sfvFiles = fileSystemDirectoryInfo.FileInfosForExtension(HandlesExtension).ToArray();

            if (sfvFiles.Length == 0)
            {
                return new OperationResult<int>("Skipping validation. No SFV files found.")
                {
                    Type = OperationResponseType.NotFound,
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

            var songPlugin = _songPlugins.First();
            foreach (var sfvFile in sfvFiles)
            {
                using (Operation.At(LogEventLevel.Debug).Time("[{Plugin}] Processing [{FileName}]", DisplayName, sfvFile.Name))
                {
                    var models = await GetModelsFromSfvFile(fileSystemDirectoryInfo, sfvFile.FullName);

                    Log.Debug("\u2502 Found [{Songs}] valid Songs for Sfv file", models.Count(x => x.IsValid));

                    var songs = new List<Common.Models.Song>();
                    await Parallel.ForEachAsync(models.Where(x => x.IsValid), cancellationToken, async (model, tt) =>
                    {
                        var songResult = await songPlugin.ProcessFileAsync(fileSystemDirectoryInfo, model.FileSystemFileInfo, tt);
                        if (songResult.IsSuccess)
                        {
                            songs.Add(songResult.Data);
                        }
                        else
                        {
                            songResult.Messages?.ForEach((x, i) => Log.Warning("[{PluginName}] Processing [{SfvModel}] Message: [{Message}]", DisplayName, model, x));
                            songResult.Errors?.ForEach((x, i) => Log.Error(x, "[{PluginName}] Processing [{SfvModel}]", DisplayName, model));
                            Log.Warning("Unable to get Song details for Sfv Model [{SfvModel}]", model);
                        }
                    });

                    if (songs.Count == 0)
                    {
                        return new OperationResult<int>($"Unable to find Songs for directory [{fileSystemDirectoryInfo}]")
                        {
                            Type = OperationResponseType.ValidationFailure,
                            Data = -1
                        };
                    }

                    var songTotal = songs.OrderByDescending(x => x.SongTotalNumber()).FirstOrDefault()?.SongTotalNumber() ?? 0;
                    if (songTotal < 1)
                    {
                        if (models.Length == songs.Count)
                        {
                            songTotal = models.Length;
                        }
                    }

                    var newAlbumTags = new List<MetaTag<object?>>
                    {
                        new() { Identifier = MetaTagIdentifier.Album, Value = songs.FirstOrDefault(x => x.AlbumTitle().Nullify() != null)?.AlbumTitle(), SortOrder = 1 },
                        new() { Identifier = MetaTagIdentifier.AlbumArtist, Value = songs.FirstOrDefault(x => x.AlbumArtist().Nullify() != null)?.AlbumArtist(), SortOrder = 2 },
                        new() { Identifier = MetaTagIdentifier.DiscNumber, Value = songs.FirstOrDefault(x => x.MediaNumber() > 0)?.MediaNumber(), SortOrder = 4 },
                        new() { Identifier = MetaTagIdentifier.OrigAlbumYear, Value = songs.FirstOrDefault(x => x.AlbumYear() > -1)?.AlbumYear(), SortOrder = 100 },
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

                    var artistName = newAlbumTags.FirstOrDefault(x => x.Identifier is MetaTagIdentifier.Artist or MetaTagIdentifier.AlbumArtist)?.Value?.ToString();
                    var sfvAlbum = new Album
                    {
                        Artist = new Artist(
                            artistName ?? throw new Exception($"Invalid artist on { nameof(SimpleFileVerification) }"),
                            artistName.ToNormalizedString() ?? artistName!,
                            null),
                        Directory = fileSystemDirectoryInfo,
                        Files = new[]
                        {
                            new AlbumFile
                            {
                                AlbumFileType = AlbumFileType.MetaData,
                                ProcessedByPlugin = DisplayName,
                                FileSystemFileInfo = sfvFile.ToFileSystemInfo()
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
                    
                    var validationResult = albumValidator.ValidateAlbum(sfvAlbum);
                    sfvAlbum.ValidationMessages = validationResult.Data.Messages ?? [];
                    sfvAlbum.Status = validationResult.Data.AlbumStatus;
                    sfvAlbum.StatusReasons = validationResult.Data.AlbumStatusReasons;                    

                    var stagingAlbumDataName = Path.Combine(fileSystemDirectoryInfo.Path, sfvAlbum.ToMelodeeJsonName(MelodeeConfiguration));
                    if (File.Exists(stagingAlbumDataName))
                    {
                        var existingAlbum = serializer.Deserialize<Album?>(await File.ReadAllTextAsync(stagingAlbumDataName, cancellationToken));
                        if (existingAlbum != null)
                        {
                            sfvAlbum = sfvAlbum.Merge(existingAlbum);
                        }
                    }

                    var serialized = serializer.Serialize(sfvAlbum);
                    await File.WriteAllTextAsync(stagingAlbumDataName, serialized, cancellationToken);
                    if (SafeParser.ToBoolean(Configuration[SettingRegistry.ProcessingDoDeleteOriginal]))
                    {
                        sfvFile.Delete();
                        Log.Information("Deleted SFV File [{FileName}]", sfvFile.Name);
                    }

                    Log.Debug("[{Plugin}] created [{StagingAlbumDataName}] Status [{Status}]", DisplayName, sfvAlbum.ToMelodeeJsonName(MelodeeConfiguration), sfvAlbum.Status.ToString());
                    processedFiles++;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "[{Name}] processing directory [{DirName}]", DisplayName, fileSystemDirectoryInfo);
            resultType = OperationResponseType.Error;
            StopProcessing = true;
        }

        return new OperationResult<int>
        {
            Type = resultType,
            Data = processedFiles
        };
    }

    public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        return fileSystemInfo.Extension(directoryInfo).DoStringsMatch(HandlesExtension);
    }

    private static async Task<SfvLine[]> GetModelsFromSfvFile(FileSystemDirectoryInfo directoryInfo, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return [];
        }

        var result = new List<SfvLine>();
        try
        {
            foreach (var line in await File.ReadAllLinesAsync(filePath))
            {
                if (IsLineForFileForSong(line))
                {
                    var model = ModelFromSfvLine(filePath, line);
                    if (model != null)
                    {
                        result.Add(model);
                    }
                }
                else
                {
                    Log.Debug($"Skipped SFV line [{line}]", line);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "FilePath [{FilePath}]", filePath);
        }

        return result.ToArray();
    }

    public static SfvLine? ModelFromSfvLine(string filePath, string? lineFromFile)
    {
        if (string.IsNullOrWhiteSpace(lineFromFile))
        {
            return null;
        }

        var directoryInfo = filePath.ToDirectoryInfo();

        try
        {
            // Sfv line is '<fileName> <CRC>'
            var lastSpace = lineFromFile.LastIndexOf(' ');
            var filename = lineFromFile.Substring(0, lastSpace);
            var crc = lineFromFile.Substring(lastSpace + 1);
            var fileSystemInfoFile = new FileSystemFileInfo
            {
                Name = filename,
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

                fileSystemInfoFile = new FileInfo(Path.Combine(dirName ?? string.Empty, filename)).ToFileSystemInfo();
            }

            var isCrcHashAccurate = IsCrCHashAccurate(fileSystemInfoFile.FullName(directoryInfo), crc);
            return new SfvLine
            {
                IsValid = !string.IsNullOrWhiteSpace(filePath) && isCrcHashAccurate,
                CrcHash = crc,
                FileSystemFileInfo = fileSystemInfoFile
            };
        }
        catch (Exception e)
        {
            Log.Error(e, "lineFromFile [{LineFromFile}]", lineFromFile);
        }

        return null;
    }

    private static bool IsCrCHashAccurate(string filename, string crcHash)
    {
        if (string.IsNullOrWhiteSpace(crcHash))
        {
            return false;
        }

        try
        {
            var fi = new FileInfo(filename);
            if (fi.Exists)
            {
                var calculated = Crc32.Calculate(fi);
                var doesMatch = string.Equals(calculated, crcHash, StringComparison.OrdinalIgnoreCase);

                Trace.WriteLine($"IsCrCHashAccurate File [{filename}] DoesMatch [{doesMatch}] Expected [{crcHash}] Calculated [{calculated}]", "Information");

                return doesMatch;
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
        }

        return false;
    }


    private static bool IsLineForFileForSong(string? lineFromFile)
    {
        if (lineFromFile?.Nullify() == null)
        {
            return false;
        }

        try
        {
            if (lineFromFile.StartsWith("#") || lineFromFile.StartsWith(";"))
            {
                return false;
            }

            var lineParts = lineFromFile.Split(' ');
            var filename = string.Join(string.Empty, lineParts[..^1]);
            var ext = Path.GetExtension(filename);
            return FileHelper.IsFileMediaType(ext);
        }
        catch (Exception e)
        {
            Log.Error(e, "lineFromFile [{LineFromFile}]", lineFromFile);
        }

        return false;
    }
}
