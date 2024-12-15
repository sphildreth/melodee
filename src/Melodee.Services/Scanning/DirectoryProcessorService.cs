using System.Diagnostics;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Plugins.Conversion;
using Melodee.Plugins.Conversion.Image;
using Melodee.Plugins.Conversion.Media;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Directory.Nfo;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Processor.Models;
using Melodee.Plugins.Scripting;
using Melodee.Plugins.Validation;
using Melodee.Services.Interfaces;
using Melodee.Services.SearchEngines;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using SixLabors.ImageSharp;
using SmartFormat;
using ImageInfo = Melodee.Common.Models.ImageInfo;

namespace Melodee.Services.Scanning;

/// <summary>
///     Take a given directory and process all the directories in it.
/// </summary>
public sealed class DirectoryProcessorService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ISettingService settingService,
    ILibraryService libraryService,
    ISerializer serializer,
    MediaEditService mediaEditService,
    ArtistSearchEngineService artistSearchEngineService,
    AlbumImageSearchEngineService albumImageSearchEngineService,
    IHttpClientFactory httpClientFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private IAlbumValidator _albumValidator = new AlbumValidator(new MelodeeConfiguration([]));
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);
    private IEnumerable<IConversionPlugin> _conversionPlugins = [];
    private IEnumerable<IDirectoryPlugin> _directoryPlugins = [];

    private string _directoryStaging = null!;
    private IImageValidator _imageValidator = new ImageValidator(new MelodeeConfiguration([]));
    private ImageConvertor _imageConvertor = new ImageConvertor(new MelodeeConfiguration([]));
    private bool _initialized;
    private int _maxAlbumProcessingCount;
    private short _maxImageCount;
    private IScriptPlugin _postDiscoveryScript = new NullScript();

    private IScriptPlugin _preDiscoveryScript = new NullScript();

    private ISongPlugin[] _songPlugins = [];
    private bool _stopProcessingTriggered;

    public async Task InitializeAsync(IMelodeeConfiguration? configuration = null, CancellationToken token = default)
    {
        if (_initialized)
        {
            return;
        }

        _configuration = configuration ?? await settingService.GetMelodeeConfigurationAsync(token).ConfigureAwait(false);

        _maxAlbumProcessingCount = _configuration.GetValue<int>(SettingRegistry.ProcessingMaximumProcessingCount, value => value < 1 ? int.MaxValue : value);
        _maxImageCount = _configuration.GetValue<short>(SettingRegistry.ImagingMaximumNumberOfAlbumImages, value => value < 1 ? SafeParser.ToNumber<short>(short.MaxValue.ToString().Length) : SafeParser.ToNumber<short>(value.ToString().Length));

        _directoryStaging = (await libraryService.GetStagingLibraryAsync(token)).Data.Path;

        _albumValidator = new AlbumValidator(_configuration);
        _imageValidator = new ImageValidator(_configuration);
        _imageConvertor = new ImageConvertor(_configuration);
        _songPlugins =
        [
            new AtlMetaTag(new MetaTagsProcessor(_configuration, serializer),_imageConvertor, _imageValidator, _configuration)
        ];

        _conversionPlugins =
        [
            new ImageConvertor(_configuration),
            new MediaConvertor(_configuration)
        ];

        _directoryPlugins =
        [
            new CueSheet(serializer, _songPlugins, _configuration)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.PluginEnabledCueSheet)
            },
            new SimpleFileVerification(serializer, _songPlugins, _albumValidator, _configuration)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.PluginEnabledSimpleFileVerification)
            },
            new M3UPlaylist(serializer, _songPlugins, _albumValidator, _configuration)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.PluginEnabledM3u)
            },
            new Nfo(serializer, _configuration)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.PluginEnabledNfo)
            }
        ];
        var preDiscoveryScript = _configuration.GetValue<string>(SettingRegistry.ScriptingPreDiscoveryScript).Nullify();
        if (preDiscoveryScript != null)
        {
            _preDiscoveryScript = new PreDiscoveryScript(_configuration);
        }

        var postDiscoveryScript = _configuration.GetValue<string>(SettingRegistry.ScriptingPostDiscoveryScript).Nullify();
        if (postDiscoveryScript != null)
        {
            _postDiscoveryScript = new PostDiscoveryScript(_configuration);
        }

        await mediaEditService.InitializeAsync(configuration, token).ConfigureAwait(false);
        await artistSearchEngineService.InitializeAsync(configuration, token).ConfigureAwait(false);

        _initialized = true;
    }

    private void CheckInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Directory processor service is not initialized.");
        }
    }

    public async Task<OperationResult<DirectoryProcessorResult>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, Instant? lastProcessDate, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var processingMessages = new List<string>();
        var processingErrors = new List<Exception>();
        var numberOfAlbumJsonFilesProcessed = 0;
        var numberOfValidAlbumsProcessed = 0;
        var numberOfAlbumsProcessed = 0;

        var conversionPluginsProcessedFileCount = 0;
        var directoryPluginProcessedFileCount = 0;
        var numberOfAlbumFilesProcessed = 0;

        var artistsUniqueIdsSeen = new List<long>();
        var albumsUniqueIdsSeen = new List<long>();
        var songsUniqueIdsSeen = new List<long>();

        var result = new DirectoryProcessorResult
        {
            DurationInMs = 0,
            NewAlbumsCount = 0,
            NewArtistsCount = 0,
            NewSongsCount = 0,
            NumberOfAlbumFilesProcessed = 0,
            NumberOfConversionPluginsProcessed = 0,
            NumberOfConversionPluginsProcessedFileCount = 0,
            NumberOfDirectoryPluginProcessed = 0,
            NumberOfValidAlbumsProcessed = 0,
            NumberOfAlbumsProcessed = 0
        };

        var startTicks = Stopwatch.GetTimestamp();

        // Ensure directory to process exists
        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (!dirInfo.Exists)
        {
            return new OperationResult<DirectoryProcessorResult>
            {
                Errors =
                [
                    new Exception($"Directory [{fileSystemDirectoryInfo}] not found.")
                ],
                Data = result
            };
        }

        // Ensure that staging directory exists
        if (!Directory.Exists(_directoryStaging))
        {
            return new OperationResult<DirectoryProcessorResult>
            {
                Errors =
                [
                    new Exception($"Staging Directory [{_directoryStaging}] not found.")
                ],
                Data = result
            };
        }

        // Run PreDiscovery script
        if (!_configuration.GetValue<bool>(SettingRegistry.ScriptingEnabled) && _preDiscoveryScript.IsEnabled)
        {
            LogAndRaiseEvent(LogEventLevel.Debug, "Executing _preDiscoveryScript [{0}]", null, _preDiscoveryScript.DisplayName);
            var preDiscoveryScriptResult = new OperationResult<bool>
            {
                Data = false
            };
            try
            {
                preDiscoveryScriptResult = await _preDiscoveryScript.ProcessAsync(fileSystemDirectoryInfo, cancellationToken);
            }
            catch (Exception e)
            {
                LogAndRaiseEvent(LogEventLevel.Error, "PreDiscoveryScript [{0}]", e, _preDiscoveryScript.DisplayName);
                preDiscoveryScriptResult.AddError(e);
            }

            if (!preDiscoveryScriptResult.IsSuccess)
            {
                return new OperationResult<DirectoryProcessorResult>(preDiscoveryScriptResult.Messages)
                {
                    Errors = preDiscoveryScriptResult.Errors,
                    Data = result
                };
            }
        }

        var directoriesToProcess = fileSystemDirectoryInfo.GetFileSystemDirectoryInfosToProcess(lastProcessDate, SearchOption.AllDirectories).ToList();
        if (directoriesToProcess.Count > 0)
        {
            OnProcessingStart?.Invoke(this, directoriesToProcess.Count);
            LogAndRaiseEvent(LogEventLevel.Debug, "\u251c Found [{0}] directories to process", null, directoriesToProcess.Count);
        }

        var httpClient = httpClientFactory.CreateClient();

        foreach (var directoryInfoToProcess in directoriesToProcess)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                {
                    break;
                }

                var allFilesInDirectory = directoryInfoToProcess.FileInfosForExtension("*").ToArray();

                LogAndRaiseEvent(LogEventLevel.Debug, "\u251c Processing [{0}] Number of files to process [{1}]", null, directoryInfoToProcess.Name, allFilesInDirectory.Length);

                // Run all enabled IDirectoryPlugins to convert MetaData files into Album json files.
                // e.g. Build Album json file for M3U or NFO or SFV, etc.
                foreach (var plugin in _directoryPlugins.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    var pluginResult = await plugin.ProcessDirectoryAsync(directoryInfoToProcess, cancellationToken);
                    if (!pluginResult.IsSuccess && pluginResult.Type != OperationResponseType.NotFound)
                    {
                        processingErrors.AddRange(pluginResult.Errors ?? []);
                        if (plugin.StopProcessing)
                        {
                            Logger.Debug("Received stop processing from [{PluginName}] on Directory [{DirectoryName}]", plugin.DisplayName, directoryInfoToProcess);
                            break;
                        }

                        continue;
                    }

                    if (pluginResult.Data > 0)
                    {
                        directoryPluginProcessedFileCount += pluginResult.Data;
                    }

                    if (plugin.StopProcessing)
                    {
                        Logger.Debug("Received stop processing from [{PluginName}] on Directory [{DirectoryName}]", plugin.DisplayName, directoryInfoToProcess);
                        break;
                    }
                }
                
                
                // Run Enabled Conversion scripts on each file in directory
                // e.g. Convert FLAC to MP3, Convert non JPEG files into JPEGs, etc.
                foreach (var fileSystemInfo in allFilesInDirectory)
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    var fsi = fileSystemInfo.ToFileSystemInfo();
                    foreach (var plugin in _conversionPlugins.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
                    {
                        if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                        {
                            break;
                        }

                        if (plugin.DoesHandleFile(directoryInfoToProcess, fsi))
                        {
                            var pluginResult = await plugin.ProcessFileAsync(directoryInfoToProcess, fsi, cancellationToken);
                            if (!pluginResult.IsSuccess)
                            {
                                processingErrors.AddRange(pluginResult.Errors ?? []);
                                processingMessages.AddRange(pluginResult.Messages ?? []);
                            }
                            else
                            {
                                conversionPluginsProcessedFileCount++;
                            }
                        }

                        if (plugin.StopProcessing)
                        {
                            break;
                        }
                    }
                }

                // Check if any Album json files exist in given directory, if none then create from Song files.
                var albumJsonFiles = directoryInfoToProcess.FileInfosForExtension(Album.JsonFileName);
                if (!albumJsonFiles.Any())
                {
                    var albumsForDirectory = await AllAlbumsForDirectoryAsync(
                        directoryInfoToProcess,
                        _songPlugins.ToArray(),
                        _configuration,
                        cancellationToken);
                    if (!albumsForDirectory.IsSuccess)
                    {
                        return new OperationResult<DirectoryProcessorResult>(albumsForDirectory.Messages)
                        {
                            Errors = albumsForDirectory.Errors,
                            Data = result
                        };
                    }

                    numberOfAlbumFilesProcessed = albumsForDirectory.Data.Item2;

                    foreach (var albumForDirectory in albumsForDirectory.Data.Item1.ToArray())
                    {
                        if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                        {
                            break;
                        }

                        var mergedAlbum = albumForDirectory;

                        var serialized = string.Empty;
                        var albumDataName = Path.Combine(directoryInfoToProcess.Path, albumForDirectory.ToMelodeeJsonName());
                        if (File.Exists(albumDataName))
                        {
                            if (_configuration.GetValue<bool>(SettingRegistry.ProcessingDoOverrideExistingMelodeeDataFiles))
                            {
                                File.Delete(albumDataName);
                            }
                            else
                            {
                                Album? existingAlbum = null;
                                try
                                {
                                    existingAlbum = serializer.Deserialize<Album?>(await File.ReadAllTextAsync(albumDataName, cancellationToken));
                                }
                                catch (Exception e)
                                {
                                    LogAndRaiseEvent(LogEventLevel.Error, "Error Deserialize melodee json file {0}]", e, albumDataName);
                                }

                                if (existingAlbum != null)
                                {
                                    mergedAlbum = mergedAlbum.Merge(existingAlbum);
                                }
                            }
                        }

                        try
                        {
                            serialized = serializer.Serialize(mergedAlbum);
                        }
                        catch (Exception e)
                        {
                            LogAndRaiseEvent(LogEventLevel.Error, "Error processing [{0}]", e, fileSystemDirectoryInfo);
                        }

                        await File.WriteAllTextAsync(albumDataName, serialized, cancellationToken);

                        numberOfAlbumJsonFilesProcessed++;

                        if (numberOfAlbumJsonFilesProcessed > _maxAlbumProcessingCount)
                        {
                            break;
                        }
                    }
                }

                // Find all Album json files in given directory, if none bail.
                albumJsonFiles = directoryInfoToProcess.FileInfosForExtension(Album.JsonFileName).ToArray();
                if (!albumJsonFiles.Any())
                {
                    processingMessages.Add($"No Albums found in directory [{directoryInfoToProcess}]");
                    continue;
                }

                // For each Album json find all image files and add to Album to be moved below to staging directory.
                var albumAndJsonFile = new Dictionary<Album, string>();
                foreach (var albumJsonFile in albumJsonFiles.Take(_maxAlbumProcessingCount))
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    try
                    {
                        Album? album = null;
                        try
                        {
                            album = serializer.Deserialize<Album>(await File.ReadAllTextAsync(albumJsonFile.FullName, cancellationToken));
                        }
                        catch (Exception e)
                        {
                            LogAndRaiseEvent(LogEventLevel.Error, "Error Deserialize melodee json file {0}]", e, albumJsonFile.FullName);
                        }

                        if (album == null)
                        {
                            return new OperationResult<DirectoryProcessorResult>($"Invalid Album json file [{albumJsonFile.FullName}]")
                            {
                                Data = result
                            };
                        }

                        var albumImages = new List<ImageInfo>();
                        var foundAlbumImages = (await FindImagesForAlbum(album, _imageConvertor, _imageValidator, _maxImageCount, cancellationToken)).ToArray();
                        if (foundAlbumImages.Length != 0)
                        {
                            foreach (var foundAlbumImage in foundAlbumImages)
                            {
                                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                                {
                                    break;
                                }

                                if (!albumImages.Any(x => x.IsCrcHashMatch(foundAlbumImage.CrcHash)))
                                {
                                    albumImages.Add(foundAlbumImage);
                                }
                            }
                        }

                        album.Images = albumImages.ToArray();

                        // Look in the album directory and see if there are any artist images. 
                        // Most of the time an artist image is one up from an album folder in the 'artist' folder.
                        var artistImages = new List<ImageInfo>();
                        var foundArtistImages = (await FindImagesForArtist(album, _imageConvertor, _imageValidator, _maxImageCount, cancellationToken)).ToArray();
                        if (foundArtistImages.Length != 0)
                        {
                            foreach (var foundArtistImage in foundArtistImages)
                            {
                                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                                {
                                    break;
                                }

                                if (!artistImages.Any(x => x.IsCrcHashMatch(foundArtistImage.CrcHash)))
                                {
                                    artistImages.Add(foundArtistImage);
                                }
                            }
                        }

                        album.Artist = new Artist(album.Artist.Name, album.Artist.NameNormalized, album.Artist.SortName, artistImages);

                        if (album.Songs != null)
                        {
                            // Set SongNumber to invalid range if SongNumber is missing
                            var maximumSongNumber = _configuration.GetValue<int>(SettingRegistry.ValidationMaximumSongNumber);
                            album.Songs.Where(x => x.SongNumber() < 1).ForEach((x, i) => { album.SetSongTagValue(x.SongId, MetaTagIdentifier.TrackNumber, maximumSongNumber + i + 1); });
                            foreach (var song in album.Songs)
                            {
                                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                                {
                                    break;
                                }

                                song.File.Name = song.ToSongFileName(album.Directory, _configuration.Configuration);
                            }
                        }

                        albumAndJsonFile.Add(album, albumJsonFile.FullName);
                    }
                    catch (Exception e)
                    {
                        processingErrors.Add(e);
                        LogAndRaiseEvent(LogEventLevel.Error, "Unable to load Album json [{0}]", e, albumJsonFile.FullName);
                    }
                }

                // Create directory and move files for each found Album in staging directory.
                foreach (var albumKvp in albumAndJsonFile)
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    var album = albumKvp.Key;
                    var albumDirInfo = new DirectoryInfo(Path.Combine(_directoryStaging, album.ToDirectoryName()));
                    if (!albumDirInfo.Exists)
                    {
                        albumDirInfo.Create();
                    }

                    var albumImages = album.Images?.Where(x => x.FileInfo?.OriginalName != null) ?? [];
                    var artistImages = album.Artist.Images?.Where(x => x.FileInfo?.OriginalName != null) ?? [];
                    foreach (var (image, _) in albumImages.Concat(artistImages).Select((image, index) => (image, index)))
                    {
                        var oldImageFileName = Path.Combine(albumKvp.Key.Directory.FullName(), image.FileInfo!.OriginalName!);
                        if (!File.Exists(oldImageFileName))
                        {
                            Logger.Warning("Unable to find image by original name [{OriginalName}]", oldImageFileName);
                            continue;
                        }

                        var newImageFileName = Path.Combine(albumDirInfo.FullName, image.FileInfo.Name);
                        if (!string.Equals(oldImageFileName, newImageFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Copy(oldImageFileName, newImageFileName, true);
                            if (_configuration.GetValue<bool>(SettingRegistry.ProcessingDoDeleteOriginal))
                            {
                                File.Delete(image.FileInfo!.FullName(albumKvp.Key.Directory));
                            }

                            image.FileInfo!.Name = Path.GetFileName(newImageFileName);
                        }
                    }

                    if (album.Songs != null)
                    {
                        foreach (var song in album.Songs.Where(x => x.File.OriginalName != null))
                        {
                            if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                            {
                                break;
                            }

                            var oldSongFilename = Path.Combine(albumKvp.Key.Directory.FullName(), song.File.OriginalName!);
                            if (!File.Exists(oldSongFilename))
                            {
                                Logger.Warning("Unable to find song by original name [{OriginalName}]", oldSongFilename);
                                continue;
                            }

                            var newSongFileName = Path.Combine(albumDirInfo.FullName, song.File.Name);
                            if (!string.Equals(oldSongFilename, newSongFileName, StringComparison.OrdinalIgnoreCase))
                            {
                                File.Copy(oldSongFilename, newSongFileName, true);
                                if (_configuration.GetValue<bool>(SettingRegistry.ProcessingDoDeleteOriginal))
                                {
                                    try
                                    {
                                        File.Delete(oldSongFilename);
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Warning(e, "Error deleting original file [{0}]", oldSongFilename);
                                    }
                                }

                                song.File.Name = Path.GetFileName(newSongFileName);
                            }
                        }

                        if ((album.Tags ?? Array.Empty<MetaTag<object?>>()).Any(x => x.WasModified) ||
                            album.Songs!.Any(x => (x.Tags ?? Array.Empty<MetaTag<object?>>()).Any(y => y.WasModified)))
                        {
                            var albumDirectorySystemInfo = albumDirInfo.ToDirectorySystemInfo();
                            foreach (var songPlugin in _songPlugins)
                            {
                                foreach (var song in album.Songs.Where(x => x.Tags?.Any(t => t.WasModified) ?? false))
                                {
                                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                                    {
                                        break;
                                    }

                                    await songPlugin.UpdateSongAsync(albumDirectorySystemInfo, song, cancellationToken);
                                }
                            }
                        }
                    }

                    album.Directory = albumDirInfo.ToDirectorySystemInfo();

                    // See if artist can be found using ArtistSearchEngine to populate metadata, set UniqueId and MusicBrainzId
                    var searchRequest = album.Artist.ToArtistQuery([
                        new KeyValue((album.AlbumYear() ?? 0).ToString(),
                            album.AlbumTitle().ToNormalizedString() ?? album.AlbumTitle())
                    ]);
                    var artistSearchResult = await artistSearchEngineService.DoSearchAsync(searchRequest,
                            1,
                            cancellationToken)
                        .ConfigureAwait(false);
                    if (artistSearchResult.IsSuccess)
                    {
                        var artistFromSearch = artistSearchResult.Data.FirstOrDefault();
                        if (artistFromSearch != null)
                        {
                            // TODO put this back in the job as this doesn't work well with having local MB be the first result and it doesnt have any images

                            // var artistFromSearchImageFilename = Path.Combine(albumDirInfo.FullName, albumDirInfo.ToDirectorySystemInfo().GetNextFileNameForType(_maxImageCount, Common.Data.Models.Artist.ImageType).Item1);
                            // if (artistFromSearch.ImageUrl != null)
                            // {
                            //     await httpClient.DownloadFileAsync(
                            //         artistFromSearch.ImageUrl,
                            //         artistFromSearchImageFilename,
                            //         async (existingFileInfo, newFileInfo, ct) =>
                            //         {
                            //             if (!(await _imageValidator.ValidateImage(newFileInfo, cancellationToken)).Data.IsValid)
                            //             {
                            //                 return false;
                            //             }
                            //
                            //             var existingImageInfo = await Image.LoadAsync(existingFileInfo.FullName, ct);
                            //             var newImageInfo = await Image.LoadAsync(newFileInfo.FullName, ct);
                            //             if (newImageInfo.Size.Height > existingImageInfo.Size.Height &&
                            //                 existingImageInfo.Size.Width > newImageInfo.Size.Width)
                            //             {
                            //                 return true;
                            //             }
                            //
                            //             return false;
                            //         },
                            //         cancellationToken);
                            // }

                            album.Artist = album.Artist with
                            {
                                MusicBrainzId = artistFromSearch.MusicBrainzId?.ToString() ?? album.Artist.MusicBrainzId,
                                Name = artistFromSearch.Name,
                                NameNormalized = artistFromSearch.Name.ToNormalizedString() ?? artistFromSearch.Name,
                                SortName = artistFromSearch.SortName,
                                SearchEngineResultUniqueId = artistFromSearch.UniqueId,
                                OriginalName = artistFromSearch.Name != album.Artist.Name ? album.Artist.Name : null
                            };

                            if (artistFromSearch.Releases?.Length != 0)
                            {
                                album.MusicBrainzId = artistFromSearch.Releases!.First().MusicBrainzId?.ToString();
                            }

                            LogAndRaiseEvent(LogEventLevel.Information, $"[{nameof(DirectoryProcessorService)}] Using artist from search engine query [{searchRequest}] result [{artistFromSearch}]");
                        }
                        else
                        {
                            LogAndRaiseEvent(LogEventLevel.Warning, $"[{nameof(DirectoryProcessorService)}] No result from search engine for artist [{searchRequest}]");
                        }
                    }

                    //
                    // TODO this doesn't work well with search providers, rate limiting and scanning 10k albums
                    //
                    // // If album has no images then see if ImageSearchEngine can find any
                    // if (album.Images?.Count() == 0)
                    // {
                    //     var albumImageSearchRequest = album.ToAlbumQuery();
                    //     var albumImageSearchResult = await albumImageSearchEngineService.DoSearchAsync(albumImageSearchRequest,
                    //             1,
                    //             cancellationToken)
                    //         .ConfigureAwait(false);
                    //     if (albumImageSearchResult.IsSuccess)
                    //     {
                    //         var imageSearchResult = albumImageSearchResult.Data.FirstOrDefault();
                    //         if (imageSearchResult != null)
                    //         {
                    //             var albumImageFromSearchFileName = Path.Combine(albumDirInfo.FullName, albumDirInfo.ToDirectorySystemInfo().GetNextFileNameForType(_maxImageCount, Common.Data.Models.Album.FrontImageType).Item1);
                    //             if (await httpClient.DownloadFileAsync(
                    //                     imageSearchResult.MediaUrl,
                    //                     albumImageFromSearchFileName,
                    //                     async (existingFileInfo, newFileInfo, ct) => (await _imageValidator.ValidateImage(newFileInfo, cancellationToken)).Data.IsValid,
                    //                     cancellationToken))
                    //             {
                    //                 var newImageInfo = new FileInfo(albumImageFromSearchFileName);
                    //                 var imageInfo = await Image.IdentifyAsync(albumImageFromSearchFileName, cancellationToken).ConfigureAwait(false);
                    //                 album.Images = new List<ImageInfo>
                    //                 {
                    //                     new()
                    //                     {
                    //                         FileInfo = newImageInfo.ToFileSystemInfo(),
                    //                         PictureIdentifier = PictureIdentifier.Front,
                    //                         CrcHash = Crc32.Calculate(newImageInfo),
                    //                         Width = imageInfo.Width,
                    //                         Height = imageInfo.Height,
                    //                         SortOrder = 1,
                    //                         WasEmbeddedInSong = false
                    //                     }
                    //                 };
                    //                 if (imageSearchResult.Rank < 1)
                    //                 {
                    //                     // without a positive match, add a validation message to manually review downloaded album image
                    //                     album.ValidationMessages = album.ValidationMessages.Append(new ValidationResultMessage
                    //                     {
                    //                         Message = "Album image needs reviewing.",
                    //                         Severity = ValidationResultMessageSeverity.Critical
                    //                     });
                    //                 }
                    //
                    //                 LogAndRaiseEvent(LogEventLevel.Information, $"[{nameof(DirectoryProcessorService)}] Downloaded album image [{imageSearchResult.MediaUrl}]");
                    //             }
                    //         }
                    //         else
                    //         {
                    //             LogAndRaiseEvent(LogEventLevel.Warning, $"[{nameof(DirectoryProcessorService)}] No result from album search engine for album [{albumImageSearchRequest}]");
                    //         }
                    //     }
                    // }

                    // Validate using AlbumValidator
                    var validationResult = _albumValidator.ValidateAlbum(album);
                    album.ValidationMessages = validationResult.Data.Messages ?? [];
                    album.Status = validationResult.Data.AlbumStatus;

                    var serialized = serializer.Serialize(album);
                    var jsonName = album.ToMelodeeJsonName(true);
                    if (jsonName.Nullify() != null)
                    {
                        await File.WriteAllTextAsync(Path.Combine(albumDirInfo.FullName, jsonName), serialized, cancellationToken);
                        if (_configuration.GetValue<bool>(SettingRegistry.ProcessingDoDeleteOriginal))
                        {
                            File.Delete(albumKvp.Value);
                        }

                        if (_configuration.GetValue<bool>(SettingRegistry.MagicEnabled))
                        {
                            using (Operation.At(LogEventLevel.Debug).Time("ProcessDirectoryAsync \ud83e\ude84 DoMagic [{DirectoryInfo}]", albumDirInfo.Name))
                            {
                                await mediaEditService.DoMagic(album.Directory, album.UniqueId(), cancellationToken);
                            }
                        }

                        artistsUniqueIdsSeen.Add(album.Artist.UniqueId());
                        artistsUniqueIdsSeen.AddRange(album.Songs?.Where(x => x.SongArtistUniqueId() != null).Select(x => x.SongArtistUniqueId() ?? 0) ?? []);
                        albumsUniqueIdsSeen.Add(album.UniqueId());
                        songsUniqueIdsSeen.AddRange(album.Songs?.Select(x => x.UniqueId) ?? []);
                    }
                    else
                    {
                        processingMessages.Add($"Unable to determine JsonName for Album [{album}]");
                    }

                    numberOfAlbumsProcessed++;

                    if (album.Status == AlbumStatus.Ok)
                    {
                        numberOfValidAlbumsProcessed++;
                        LogAndRaiseEvent(LogEventLevel.Debug, $"[{nameof(DirectoryProcessorService)}] * Found valid album [{fileSystemDirectoryInfo}]");
                        if (numberOfValidAlbumsProcessed >= _maxAlbumProcessingCount)
                        {
                            LogAndRaiseEvent(LogEventLevel.Information, $"[{nameof(DirectoryProcessorService)}] \ud83d\uded1 Stopped processing directory [{fileSystemDirectoryInfo}], processing.maximumProcessingCount is set to [{_maxAlbumProcessingCount}]");
                            _stopProcessingTriggered = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogAndRaiseEvent(LogEventLevel.Error, "Processing Directory [{0}]", e, directoryInfoToProcess.ToString());
                processingErrors.Add(e);
                if (!_configuration.GetValue<bool>(SettingRegistry.ProcessingDoContinueOnDirectoryProcessingErrors))
                {
                    return new OperationResult<DirectoryProcessorResult>
                    {
                        Errors = processingErrors,
                        Data = result
                    };
                }
            }

            OnDirectoryProcessed?.Invoke(this, directoryInfoToProcess);
        }

        // Run PostDiscovery script
        if (!_configuration.GetValue<bool>(SettingRegistry.ScriptingEnabled) && _postDiscoveryScript.IsEnabled)
        {
            var postDiscoveryScriptResult = new OperationResult<bool>
            {
                Data = false
            };
            try
            {
                postDiscoveryScriptResult = await _postDiscoveryScript.ProcessAsync(fileSystemDirectoryInfo, cancellationToken);
            }
            catch (Exception e)
            {
                LogAndRaiseEvent(LogEventLevel.Error, "PostDiscoveryScript [{0}]", e, _postDiscoveryScript.DisplayName);
                postDiscoveryScriptResult.AddError(e);
            }

            if (!postDiscoveryScriptResult.IsSuccess)
            {
                return new OperationResult<DirectoryProcessorResult>(postDiscoveryScriptResult.Messages)
                {
                    Errors = postDiscoveryScriptResult.Errors,
                    Data = result
                };
            }
        }

        fileSystemDirectoryInfo.DeleteAllEmptyDirectories();
        LogAndRaiseEvent(LogEventLevel.Information, "Processing Complete!");

        return new OperationResult<DirectoryProcessorResult>(processingMessages)
        {
            Errors = processingErrors.ToArray(),
            Data = new DirectoryProcessorResult
            {
                DurationInMs = Stopwatch.GetElapsedTime(startTicks).TotalMilliseconds,
                NewAlbumsCount = albumsUniqueIdsSeen.Distinct().Count(),
                NewArtistsCount = artistsUniqueIdsSeen.Distinct().Count(),
                NewSongsCount = songsUniqueIdsSeen.Distinct().Count(),
                NumberOfAlbumFilesProcessed = numberOfAlbumJsonFilesProcessed,
                NumberOfConversionPluginsProcessed = numberOfAlbumFilesProcessed,
                NumberOfConversionPluginsProcessedFileCount = conversionPluginsProcessedFileCount,
                NumberOfDirectoryPluginProcessed = directoryPluginProcessedFileCount,
                NumberOfValidAlbumsProcessed = numberOfValidAlbumsProcessed,
                NumberOfAlbumsProcessed = numberOfAlbumsProcessed
            }
        };
    }

    /// <summary>
    ///     This is raised when a Log event happens to return activity to caller.
    /// </summary>
    public event EventHandler<string>? OnProcessingEvent;

    /// <summary>
    ///     This is raised when the number of directories to process is known.
    /// </summary>
    public event EventHandler<int>? OnProcessingStart;

    /// <summary>
    ///     This is raised when a new Album is processed put into the Staging directory.
    /// </summary>
    public event EventHandler<FileSystemDirectoryInfo>? OnDirectoryProcessed;

    private void LogAndRaiseEvent(LogEventLevel logLevel, string messageTemplate, Exception? exception = null, params object[] args)
    {
        if (exception != null)
        {
            Log.Error(exception, messageTemplate, args);
        }
        else
        {
            Log.Write(logLevel, messageTemplate, args);
        }

        var eventMessage = messageTemplate;
        if (args.Length > 0)
        {
            try
            {
                eventMessage = Smart.Format(eventMessage, args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        OnProcessingEvent?.Invoke(this, exception?.ToString() ?? eventMessage);
    }

    //TODO make this use the Artist method to get images for an artist
    private static async Task<IEnumerable<ImageInfo>> FindImagesForArtist(Album album, ImageConvertor imageConvertor, IImageValidator imageValidator, short maxNumberOfImagesLength, CancellationToken cancellationToken = default)
    {
        var imageInfos = new List<ImageInfo>();
        var imageFiles = ImageHelper.ImageFilesInDirectory(album.OriginalDirectory.Path, SearchOption.TopDirectoryOnly).ToArray();
        var index = 1;
        foreach (var imageFile in imageFiles.Order())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var fileInfo = new FileInfo(imageFile);
            var fileNameNormalized = fileInfo.Name.ToNormalizedString() ?? fileInfo.Name;
            var isArtistImage = fileNameNormalized.Contains(album.Artist.NameNormalized, StringComparison.OrdinalIgnoreCase);
            if (isArtistImage || ImageHelper.IsArtistImage(fileInfo) || ImageHelper.IsArtistSecondaryImage(fileInfo))
            {
                if (!(await imageValidator.ValidateImage(fileInfo, ImageHelper.IsArtistImage(fileInfo) ? PictureIdentifier.Artist : PictureIdentifier.ArtistSecondary, cancellationToken)).Data.IsValid)
                {
                    // Try converting (resizing and padding if needed) image and then revalidate
                    await imageConvertor.ProcessFileAsync(fileInfo.ToDirectorySystemInfo(), fileInfo.ToFileSystemInfo(), cancellationToken).ConfigureAwait(false);
                    if (!(await imageValidator.ValidateImage(fileInfo, ImageHelper.IsAlbumImage(fileInfo) ? PictureIdentifier.Front : PictureIdentifier.SecondaryFront, cancellationToken)).Data.IsValid)
                    {
                        continue;
                    }
                }

                var pictureIdentifier = PictureIdentifier.NotSet;
                if (ImageHelper.IsArtistImage(fileInfo))
                {
                    pictureIdentifier = PictureIdentifier.Band;
                }
                else if (ImageHelper.IsArtistSecondaryImage(fileInfo))
                {
                    pictureIdentifier = PictureIdentifier.BandSecondary;
                }

                if (pictureIdentifier != PictureIdentifier.NotSet)
                {
                    var imageInfo = await Image.LoadAsync(fileInfo.FullName, cancellationToken);
                    var fileInfoFileSystemInfo = fileInfo.ToFileSystemInfo();
                    imageInfos.Add(new ImageInfo
                    {
                        CrcHash = Crc32.Calculate(fileInfo),
                        FileInfo = new FileSystemFileInfo
                        {
                            Name = $"{ImageInfo.ImageFilePrefix}{index.ToStringPadLeft(maxNumberOfImagesLength)}-{pictureIdentifier}.jpg",
                            Size = fileInfoFileSystemInfo.Size,
                            OriginalName = fileInfo.Name
                        },
                        OriginalFilename = fileInfo.Name,
                        PictureIdentifier = pictureIdentifier,
                        Width = imageInfo.Width,
                        Height = imageInfo.Height,
                        SortOrder = index
                    });
                    index++;
                }
            }
        }

        return imageInfos;
    }

    private static async Task<IEnumerable<ImageInfo>> FindImagesForAlbum(Album album, ImageConvertor imageConvertor, IImageValidator imageValidator, short maxNumberOfImagesLength, CancellationToken cancellationToken = default)
    {
        var imageInfos = new List<ImageInfo>();
        var imageFiles = ImageHelper.ImageFilesInDirectory(album.OriginalDirectory.Path, SearchOption.TopDirectoryOnly).ToList();
        // If there are directories in the album directory that contains images include the images in that; we don't want to do AllDirectories as there might be nested albums each with their own image folders.
        foreach (var dir in album.ImageDirectories())
        {
            imageFiles.AddRange(ImageHelper.ImageFilesInDirectory(dir.FullName, SearchOption.TopDirectoryOnly));
        }
        var index = 1;
        foreach (var imageFile in imageFiles.Order())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            var fileInfo = new FileInfo(imageFile);
            
            if (album.IsFileForAlbum(fileInfo))
            {
                // Move the image if not in the album directory
                if (fileInfo.DirectoryName != album.Directory.FullName())
                {
                    var newFileInfoName = Path.Combine(album.Directory.FullName(), $"{ Path.GetFileNameWithoutExtension(fileInfo.Name)}-{fileInfo.Directory.Name}{Path.GetExtension(fileInfo.Name)}");
                    File.Move(fileInfo.FullName, newFileInfoName);
                    fileInfo = new FileInfo(newFileInfoName);
                }  
                
                if (!(await imageValidator.ValidateImage(fileInfo, ImageHelper.IsAlbumImage(fileInfo) ? PictureIdentifier.Front : PictureIdentifier.SecondaryFront, cancellationToken)).Data.IsValid)
                {
                    // Try converting (resizing and padding if needed) image and then revalidate
                    await imageConvertor.ProcessFileAsync(fileInfo.ToDirectorySystemInfo(), fileInfo.ToFileSystemInfo(), cancellationToken).ConfigureAwait(false);
                    if (!(await imageValidator.ValidateImage(fileInfo, ImageHelper.IsAlbumImage(fileInfo) ? PictureIdentifier.Front : PictureIdentifier.SecondaryFront, cancellationToken)).Data.IsValid)
                    {
                        continue;
                    }
                }

                var fileNameNormalized = (fileInfo.Name.ToNormalizedString() ?? fileInfo.Name).Replace("AND", string.Empty);
                var artistNormalized = album.Artist.NameNormalized;
                var albumNameNormalized = album.AlbumTitle().ToNormalizedString() ?? album.AlbumTitle() ?? string.Empty;
                var isAlbumImage = fileNameNormalized.Contains(artistNormalized, StringComparison.OrdinalIgnoreCase) &&
                                   fileNameNormalized.Contains(albumNameNormalized, StringComparison.OrdinalIgnoreCase);
                if (isAlbumImage ||
                    ImageHelper.IsAlbumImage(fileInfo) ||
                    ImageHelper.IsAlbumSecondaryImage(fileInfo))
                {
                    var pictureIdentifier = PictureIdentifier.NotSet;
                    if (ImageHelper.IsAlbumImage(fileInfo) || isAlbumImage)
                    {
                        pictureIdentifier = PictureIdentifier.Front;
                    }
                    else if (ImageHelper.IsAlbumSecondaryImage(fileInfo))
                    {
                        pictureIdentifier = PictureIdentifier.SecondaryFront;
                    }

                    if (pictureIdentifier != PictureIdentifier.NotSet)
                    {
                        var imageInfo = await Image.LoadAsync(fileInfo.FullName, cancellationToken);
                        var fileInfoFileSystemInfo = fileInfo.ToFileSystemInfo();
                        imageInfos.Add(new ImageInfo
                        {
                            CrcHash = Crc32.Calculate(fileInfo),
                            FileInfo = new FileSystemFileInfo
                            {
                                //TODO use the directory helper to get next image number
                                Name = $"{ImageInfo.ImageFilePrefix}{index.ToStringPadLeft(maxNumberOfImagesLength)}-{pictureIdentifier}.jpg",
                                Size = fileInfoFileSystemInfo.Size,
                                OriginalName = fileInfo.Name
                            },
                            OriginalFilename = fileInfo.Name,
                            PictureIdentifier = pictureIdentifier,
                            Width = imageInfo.Width,
                            Height = imageInfo.Height,
                            SortOrder = index
                        });
                        index++;
                    }
                }
            }
        }

        return imageInfos;
    }
}
