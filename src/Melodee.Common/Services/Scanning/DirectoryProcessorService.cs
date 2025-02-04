using System.Diagnostics;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.SpecialArtists;
using Melodee.Common.Plugins.Conversion;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Plugins.Conversion.Media;
using Melodee.Common.Plugins.MetaData.Directory;
using Melodee.Common.Plugins.MetaData.Directory.Nfo;
using Melodee.Common.Plugins.MetaData.Song;
using Melodee.Common.Plugins.Processor;
using Melodee.Common.Plugins.Processor.Models;
using Melodee.Common.Plugins.Scripting;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Interfaces;
using Melodee.Common.Services.SearchEngines;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using SixLabors.ImageSharp;
using SmartFormat;
using ImageInfo = Melodee.Common.Models.ImageInfo;

namespace Melodee.Common.Services.Scanning;

/// <summary>
///     Take a given directory and process all the directories in it.
/// </summary>
public sealed class DirectoryProcessorService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    LibraryService libraryService,
    ISerializer serializer,
    MediaEditService mediaEditService,
    ArtistSearchEngineService artistSearchEngineService,
    AlbumImageSearchEngineService albumImageSearchEngineService,
    IHttpClientFactory httpClientFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private IAlbumValidator _albumValidator = new AlbumValidator(new MelodeeConfiguration([]));
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);

    /// <summary>
    ///     These plugins convert media from various formats into configured formats.
    /// </summary>
    private IEnumerable<IConversionPlugin> _conversionPlugins = [];

    /// <summary>
    ///     These plugins translate various files into albums.
    /// </summary>
    private IEnumerable<IDirectoryPlugin> _directoryPlugins = [];

    private string _directoryStaging = null!;
    private ImageConvertor _imageConvertor = new(new MelodeeConfiguration([]));
    private IImageValidator _imageValidator = new ImageValidator(new MelodeeConfiguration([]));
    private bool _initialized;
    private int _maxAlbumProcessingCount;
    private short _maxImageCount;

    /// <summary>
    ///     These plugins create albums from media files.
    /// </summary>
    private IEnumerable<IDirectoryPlugin> _mediaAlbumCreatorPlugins = [];

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

        _configuration = configuration ?? await configurationFactory.GetConfigurationAsync(token).ConfigureAwait(false);

        _maxAlbumProcessingCount = _configuration.GetValue<int>(SettingRegistry.ProcessingMaximumProcessingCount, value => value < 1 ? int.MaxValue : value);
        _maxImageCount = _configuration.GetValue<short>(SettingRegistry.ImagingMaximumNumberOfAlbumImages, value => value < 1 ? SafeParser.ToNumber<short>(short.MaxValue.ToString().Length) : SafeParser.ToNumber<short>(value.ToString().Length));

        _directoryStaging = (await libraryService.GetStagingLibraryAsync(token).ConfigureAwait(false)).Data.Path;

        _albumValidator = new AlbumValidator(_configuration);
        _imageValidator = new ImageValidator(_configuration);
        _imageConvertor = new ImageConvertor(_configuration);
        _songPlugins =
        [
            new AtlMetaTag(new MetaTagsProcessor(_configuration, serializer), _imageConvertor, _imageValidator, _configuration)
        ];

        _conversionPlugins =
        [
            new ImageConvertor(_configuration),
            new MediaConvertor(_configuration)
        ];

        _directoryPlugins =
        [
            new CueSheet(_songPlugins, _albumValidator, _configuration)
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
            new Nfo(serializer, _albumValidator, _configuration)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.PluginEnabledNfo)
            }
        ];

        _mediaAlbumCreatorPlugins =
        [
            new Mp3Files(_songPlugins, _albumValidator, serializer, Logger, _configuration)
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

    public async Task<OperationResult<DirectoryProcessorResult>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, Instant? lastProcessDate, int? maxAlbumsToProcess, CancellationToken cancellationToken = default)
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

        var artistsIdsSeen = new List<long?>();
        var albumsIdsSeen = new List<long?>();
        var songsIdsSeen = new List<Guid>();

        var skipPrefix = _configuration.GetValue<string>(SettingRegistry.ProcessingSkippedDirectoryPrefix);

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

        _maxAlbumProcessingCount = maxAlbumsToProcess ?? _maxAlbumProcessingCount;

        var startTicks = Stopwatch.GetTimestamp();

        // Ensure directory to process exists
        Trace.WriteLine($"Ensuring processing path [{fileSystemDirectoryInfo.Path}] exists...");
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
        Trace.WriteLine($"Ensuring staging path [{_directoryStaging}] exists...");
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
                preDiscoveryScriptResult = await _preDiscoveryScript.ProcessAsync(fileSystemDirectoryInfo, cancellationToken).ConfigureAwait(false);
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

        var directoriesToProcess = fileSystemDirectoryInfo.GetFileSystemDirectoryInfosToProcess(_configuration, lastProcessDate, SearchOption.AllDirectories).ToList();
        var mediaDirectoriesToProcess = directoriesToProcess.Where(x => x.GetParent().UniqueId != fileSystemDirectoryInfo.UniqueId && x.AllMediaTypeFileInfos().Any()).ToArray();
        if (mediaDirectoriesToProcess.Length > 0)
        {
            // This means there are subdirectories which have media files in directories which have media files, must be one album per directory.
            foreach (var mediaDirectoryToProcess in mediaDirectoriesToProcess)
            {

                try
                {
                    if (mediaDirectoryToProcess.IsAlbumMediaDirectory())
                    {
                        Logger.Debug(":: [{ServiceName}] Skipping nested album media directory [{Dir}]", nameof(DirectoryProcessorService), mediaDirectoryToProcess.FullName());
                        continue;
                    }
                    var newDir = new DirectoryInfo(Path.Combine(fileSystemDirectoryInfo.FullName(), Guid.NewGuid().ToString()));
                    mediaDirectoryToProcess.MoveToDirectory(newDir.FullName);
                    Logger.Debug(":: [{ServiceName}] :: Moved nested album [{Moved}] to [{NewName}]", nameof(DirectoryProcessorService), mediaDirectoryToProcess.FullName(), newDir.FullName);
                    directoriesToProcess.Remove(mediaDirectoryToProcess);
                    directoriesToProcess.Add(newDir.ToDirectorySystemInfo());
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error processing album media directory [{Dir}]", mediaDirectoryToProcess.FullName());
                }
            }
        }

        if (directoriesToProcess.Count > 0)
        {
            OnProcessingStart?.Invoke(this, directoriesToProcess.Count);
            LogAndRaiseEvent(LogEventLevel.Debug, "\u251c Found [{0}] directories to process", null, directoriesToProcess.Count);
        }

        var httpClient = httpClientFactory.CreateClient();

        foreach (var directoryInfoToProcess in directoriesToProcess)
        {
            Trace.WriteLine($"DirectoryInfoToProcess: [{directoryInfoToProcess}]");
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

                    var pluginResult = await plugin.ProcessDirectoryAsync(directoryInfoToProcess, cancellationToken).ConfigureAwait(false);
                    if (!pluginResult.IsSuccess && pluginResult.Type != OperationResponseType.NotFound)
                    {
                        processingErrors.AddRange(pluginResult.Errors ?? []);
                        if (plugin.StopProcessing)
                        {
                            Logger.Debug("Received stop processing from [{PluginName}] on Directory [{DirectoryName}]", plugin.DisplayName, directoryInfoToProcess);
                            if (pluginResult.Type == OperationResponseType.Error && skipPrefix.Nullify() != null)
                            {
                                var movedTo = $"{skipPrefix}{directoryInfoToProcess.Name}";
                                directoryInfoToProcess.MoveToDirectory(Path.Combine(fileSystemDirectoryInfo.FullName(), movedTo));
                                LogAndRaiseEvent(LogEventLevel.Warning, "Failed processing [{0}] moved to [{1}]", null, directoryInfoToProcess.ToString(), movedTo);
                            }

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
                            var pluginResult = await plugin.ProcessFileAsync(directoryInfoToProcess, fsi, cancellationToken).ConfigureAwait(false);
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

                // If no albums were created by previous plugins, create from media files
                if (!directoryInfoToProcess.MelodeeJsonFiles().Any())
                {
                    foreach (var plugin in _mediaAlbumCreatorPlugins.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
                    {
                        if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                        {
                            break;
                        }

                        await plugin.ProcessDirectoryAsync(directoryInfoToProcess, cancellationToken).ConfigureAwait(false);
                        if (plugin.StopProcessing)
                        {
                            Logger.Debug("Received stop processing from [{PluginName}] on Directory [{DirectoryName}]", plugin.DisplayName, directoryInfoToProcess);
                            break;
                        }
                    }
                }

                var albumsForDirectory = new List<Album>();
                foreach (var melodeeJsonFile in directoryInfoToProcess.MelodeeJsonFiles())
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    try
                    {
                        var album = await Album.DeserializeAndInitializeAlbumAsync(serializer, melodeeJsonFile.FullName, cancellationToken).ConfigureAwait(false);
                        if (album != null)
                        {
                            album.MelodeeDataFileName = melodeeJsonFile.FullName;
                            albumsForDirectory.Add(album);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Error loading Album json file [{0}]", melodeeJsonFile.FullName);
                    }
                }


                Trace.WriteLine($"Loading [{albumsForDirectory.Count}] album directories");

                // For each Album json find all image files and add to Album to be moved below to staging directory.
                //var albumAndJsonFile = new Dictionary<Album, string>();
                Trace.WriteLine("Loading images for album...");
                foreach (var album in albumsForDirectory.Take(_maxAlbumProcessingCount))
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    try
                    {
                        var albumImages = new List<ImageInfo>();
                        var foundAlbumImages = (await FindImagesForAlbum(album, _imageConvertor, _imageValidator, _maxImageCount, cancellationToken).ConfigureAwait(false)).ToArray();
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
                        // Most of the time an artist image is one up from an album directory in the 'artist' directory.
                        var artistImages = new List<ImageInfo>();
                        var foundArtistImages = (await FindImagesForArtist(album,
                            _imageConvertor,
                            _imageValidator,
                            _maxImageCount,
                            _configuration.GetValue<bool>(SettingRegistry.ProcessingDoDeleteOriginal),
                            cancellationToken).ConfigureAwait(false)).ToArray();
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
                        if (album.IsSoundTrackTypeAlbum() && album.Songs != null)
                        {
                            // If the album has different artists and is soundtrack then ensure artist is set to special VariousArtists
                            var songsGroupedByArtist = album.Songs.GroupBy(x => x.AlbumArtist()).ToArray();
                            if (songsGroupedByArtist.Length > 1)
                            {
                                album.Artist = new VariousArtist();
                                foreach (var song in album.Songs)
                                {
                                    album.SetSongTagValue(song.Id, MetaTagIdentifier.AlbumArtist, album.Artist.Name);
                                }
                            }
                        }
                        else if (album.IsOriginalCastTypeAlbum() && album.Songs != null)
                        {
                            // If the album has different artists and is Original Cast type then ensure artist is set to special Theater
                            // NOTE: Remember Original Cast Type albums with a single composer/artist is attributed to that composer/artist (e.g. Stephen Schwartz - Wicked)
                            var songsGroupedByArtist = album.Songs.GroupBy(x => x.AlbumArtist()).ToArray();
                            if (songsGroupedByArtist.Length > 1)
                            {
                                album.Artist = new Theater();
                                foreach (var song in album.Songs)
                                {
                                    album.SetSongTagValue(song.Id, MetaTagIdentifier.AlbumArtist, album.Artist.Name);
                                }
                            }
                        }

                        var albumDirectorySystemInfo = new FileSystemDirectoryInfo
                        {
                            Path = Path.Combine(_directoryStaging, album.ToDirectoryName()),
                            Name = album.ToDirectoryName()
                        };
                        albumDirectorySystemInfo.EnsureExists();

                        var albumImagesToMove = album.Images?.Where(x => x.FileInfo?.OriginalName != null) ?? [];
                        var artistImageToMove = album.Artist.Images?.Where(x => x.FileInfo?.OriginalName != null) ?? [];
                        foreach (var image in albumImagesToMove.Concat(artistImageToMove))
                        {
                            var oldImageFileName = Path.Combine(album.Directory.FullName(), image.FileInfo!.OriginalName!);
                            if (!File.Exists(oldImageFileName))
                            {
                                Logger.Warning("Unable to find image by original name [{OriginalName}]", oldImageFileName);
                                continue;
                            }

                            var imageFileInfo = new FileInfo(oldImageFileName);
                            if (albumDirectorySystemInfo.FileIsLikelyDuplicateByCrcAndExtension(imageFileInfo))
                            {
                                Logger.Debug("Skipping duplicate image [{ImageName}]", image.FileInfo!.Name);
                                continue;
                            }

                            var newImageFileName = Path.Combine(albumDirectorySystemInfo.FullName(), image.FileInfo.Name);
                            if (!string.Equals(oldImageFileName, newImageFileName, StringComparison.OrdinalIgnoreCase))
                            {
                                File.Copy(oldImageFileName, newImageFileName, true);
                                if (_configuration.GetValue<bool>(SettingRegistry.ProcessingDoDeleteOriginal))
                                {
                                    File.Delete(oldImageFileName);
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

                                var oldSongFilename = Path.Combine(album.Directory.FullName(), song.File.OriginalName!);
                                if (!File.Exists(oldSongFilename))
                                {
                                    Logger.Warning("Unable to find song by original name [{OriginalName}]", oldSongFilename);
                                    continue;
                                }

                                var newSongFileName = Path.Combine(albumDirectorySystemInfo.FullName(), song.ToSongFileName(albumDirectorySystemInfo));
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
                                Trace.WriteLine("Running plugins on songs with modified tags...");

                                foreach (var songPlugin in _songPlugins)
                                {
                                    foreach (var song in album.Songs.Where(x => x.Tags?.Any(t => t.WasModified) ?? false))
                                    {
                                        if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                                        {
                                            break;
                                        }

                                        using (Operation.At(LogEventLevel.Debug).Time("ProcessDirectoryAsync :: Updating song [{Name}] with plugin [{DisplayName}]", song.File.Name, songPlugin.DisplayName))
                                        {
                                            await songPlugin.UpdateSongAsync(albumDirectorySystemInfo, song, cancellationToken).ConfigureAwait(false);
                                        }
                                    }
                                }
                            }
                        }

                        album.Directory = albumDirectorySystemInfo;

                        // See if artist can be found using ArtistSearchEngine to populate metadata, set UniqueId and MusicBrainzId
                        Trace.WriteLine("Querying for artist...");
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
                            var artistFromSearch = artistSearchResult.Data.OrderByDescending(x => x.Rank).FirstOrDefault();
                            if (artistFromSearch != null)
                            {
                                album.Artist = album.Artist with
                                {
                                    AmgId = album.Artist.AmgId ?? artistFromSearch.AmgId,
                                    ArtistDbId = album.Artist.ArtistDbId ?? artistFromSearch.Id,
                                    DiscogsId = album.Artist.DiscogsId ?? artistFromSearch.DiscogsId,
                                    ItunesId = album.Artist.ItunesId ?? artistFromSearch.ItunesId,
                                    LastFmId = album.Artist.LastFmId ?? artistFromSearch.LastFmId,
                                    MusicBrainzId = album.Artist.MusicBrainzId ?? artistFromSearch.MusicBrainzId,
                                    Name = album.Artist.Name.Nullify() ?? artistFromSearch.Name,
                                    NameNormalized = album.Artist.NameNormalized.Nullify() ?? artistFromSearch.Name.ToNormalizedString() ?? artistFromSearch.Name,
                                    OriginalName = artistFromSearch.Name != album.Artist.Name ? album.Artist.Name : null,
                                    SearchEngineResultUniqueId = album.Artist.SearchEngineResultUniqueId is null or < 1 ? artistFromSearch.UniqueId : album.Artist.SearchEngineResultUniqueId,
                                    SortName = album.Artist.SortName ?? artistFromSearch.SortName,
                                    SpotifyId = album.Artist.SpotifyId ?? artistFromSearch.SpotifyId,
                                    WikiDataId = album.Artist.WikiDataId ?? artistFromSearch.WikiDataId
                                };

                                if (artistFromSearch.Releases?.FirstOrDefault() != null)
                                {
                                    album.AlbumDbId = album.AlbumDbId ?? artistFromSearch.Releases!.First().Id;
                                    album.AlbumType = album.AlbumType == AlbumType.NotSet ? artistFromSearch.Releases!.First().AlbumType : album.AlbumType;

                                    // Artist result should override any in place for Album as its more specific and likely more accurate
                                    album.MusicBrainzId = artistFromSearch.Releases!.First().MusicBrainzId;
                                    album.SpotifyId = artistFromSearch.Releases!.First().SpotifyId;
                                }

                                album.Status = AlbumStatus.Ok;

                                LogAndRaiseEvent(LogEventLevel.Debug, $"[{nameof(DirectoryProcessorService)}] Using artist from search engine query [{searchRequest}] result [{artistFromSearch}]");
                            }
                            else
                            {
                                LogAndRaiseEvent(LogEventLevel.Warning, $"[{nameof(DirectoryProcessorService)}] No result from search engine for artist [{searchRequest}]");
                            }
                        }

                        // If album has no images then see if ImageSearchEngine can find any
                        if (album.Images?.Count() == 0)
                        {
                            Trace.WriteLine("Querying for album image...");
                            var albumImageSearchRequest = album.ToAlbumQuery();
                            var albumImageSearchResult = await albumImageSearchEngineService.DoSearchAsync(albumImageSearchRequest,
                                    1,
                                    cancellationToken)
                                .ConfigureAwait(false);
                            if (albumImageSearchResult.IsSuccess)
                            {
                                var imageSearchResult = albumImageSearchResult.Data.OrderByDescending(x => x.Rank).FirstOrDefault();
                                if (imageSearchResult != null)
                                {
                                    album.AmgId ??= imageSearchResult.AmgId;
                                    album.DiscogsId ??= imageSearchResult.DiscogsId;
                                    album.ItunesId ??= imageSearchResult.ItunesId;
                                    album.LastFmId ??= imageSearchResult.LastFmId;
                                    album.SpotifyId ??= imageSearchResult.SpotifyId;
                                    album.WikiDataId ??= imageSearchResult.WikiDataId;

                                    album.Artist.AmgId ??= imageSearchResult.ArtistAmgId;
                                    album.Artist.DiscogsId ??= imageSearchResult.ArtistDiscogsId;
                                    album.Artist.ItunesId ??= imageSearchResult.ArtistItunesId;
                                    album.Artist.LastFmId ??= imageSearchResult.ArtistLastFmId;
                                    album.Artist.SpotifyId ??= imageSearchResult.ArtistSpotifyId;
                                    album.Artist.WikiDataId ??= imageSearchResult.ArtistWikiDataId;

                                    var albumImageFromSearchFileName = Path.Combine(albumDirectorySystemInfo.FullName(), albumDirectorySystemInfo.GetNextFileNameForType(_maxImageCount, Data.Models.Album.FrontImageType).Item1);
                                    if (await httpClient.DownloadFileAsync(
                                            imageSearchResult.MediaUrl,
                                            albumImageFromSearchFileName,
                                            async (_, newFileInfo, _) => (await _imageValidator.ValidateImage(newFileInfo, PictureIdentifier.Front, cancellationToken)).Data.IsValid,
                                            cancellationToken).ConfigureAwait(false))
                                    {
                                        var newImageInfo = new FileInfo(albumImageFromSearchFileName);
                                        var imageInfo = await Image.IdentifyAsync(albumImageFromSearchFileName, cancellationToken).ConfigureAwait(false);
                                        album.Images = new List<ImageInfo>
                                        {
                                            new()
                                            {
                                                FileInfo = newImageInfo.ToFileSystemInfo(),
                                                PictureIdentifier = PictureIdentifier.Front,
                                                CrcHash = Crc32.Calculate(newImageInfo),
                                                Width = imageInfo.Width,
                                                Height = imageInfo.Height,
                                                SortOrder = 1,
                                                WasEmbeddedInSong = false
                                            }
                                        };
                                        LogAndRaiseEvent(LogEventLevel.Debug, $"[{nameof(DirectoryProcessorService)}] Downloaded album image [{imageSearchResult.MediaUrl}]");
                                    }
                                }
                                else
                                {
                                    LogAndRaiseEvent(LogEventLevel.Warning, $"[{nameof(DirectoryProcessorService)}] No result from album search engine for album [{albumImageSearchRequest}]");
                                }
                            }
                        }

                        Trace.WriteLine("Validating album...");
                        var validationResult = _albumValidator.ValidateAlbum(album);
                        album.ValidationMessages = validationResult.Data.Messages ?? [];
                        album.Status = validationResult.Data.AlbumStatus;
                        album.StatusReasons = validationResult.Data.AlbumStatusReasons;
                        Trace.WriteLine($"Validating album: status [{album.Status.ToString()}] reasons [{(album.StatusReasons == AlbumNeedsAttentionReasons.NotSet ? "None" : album.StatusReasons.ToString())}]");

                        Trace.WriteLine("Serializing album...");
                        var serialized = serializer.Serialize(album);
                        var jsonName = album.ToMelodeeJsonName(_configuration, true);
                        if (jsonName.Nullify() != null)
                        {
                            if (album.MelodeeDataFileName != null)
                            {
                                Trace.WriteLine("Deleting MelodeeDataFileName...");
                                File.Delete(album.MelodeeDataFileName);
                            }
                            Trace.WriteLine($"Writing serialized album [{ Path.Combine(albumDirectorySystemInfo.FullName(), jsonName) }]...");
                            await File.WriteAllTextAsync(Path.Combine(albumDirectorySystemInfo.FullName(), jsonName), serialized, cancellationToken).ConfigureAwait(false);
                            if (_configuration.GetValue<bool>(SettingRegistry.MagicEnabled))
                            {
                                await mediaEditService.DoMagic(album.Directory, album.Id, cancellationToken).ConfigureAwait(false);
                            }
                            artistsIdsSeen.Add(album.Artist.ArtistUniqueId());
                            artistsIdsSeen.AddRange(album.Songs?.Where(x => x.SongArtistUniqueId() != null).Select(x => x.SongArtistUniqueId()) ?? []);
                            albumsIdsSeen.Add(album.ArtistAlbumUniqueId());
                            songsIdsSeen.AddRange(album.Songs?.Select(x => x.Id) ?? []);
                        }
                        else
                        {
                            processingMessages.Add($"Unable to determine JsonName for Album [{album}]");
                        }

                        numberOfAlbumsProcessed++;

                        if (album.IsValid)
                        {
                            numberOfValidAlbumsProcessed++;
                            LogAndRaiseEvent(LogEventLevel.Debug, $"[{nameof(DirectoryProcessorService)}] \ud83d\udc4d Found valid album [{album}]");
                            if (numberOfValidAlbumsProcessed >= _maxAlbumProcessingCount)
                            {
                                LogAndRaiseEvent(LogEventLevel.Debug, $"[{nameof(DirectoryProcessorService)}] \ud83d\uded1 Stopped processing directory [{fileSystemDirectoryInfo}], processing.maximumProcessingCount is set to [{_maxAlbumProcessingCount}]");
                                _stopProcessingTriggered = true;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogAndRaiseEvent(LogEventLevel.Error, $"[{nameof(DirectoryProcessorService)}] Error processing directory [{fileSystemDirectoryInfo}]", e);
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

                if (skipPrefix.Nullify() != null)
                {
                    var movedTo = $"{skipPrefix}{directoryInfoToProcess.Name}";
                    directoryInfoToProcess.MoveToDirectory(Path.Combine(fileSystemDirectoryInfo.FullName(), movedTo));
                    LogAndRaiseEvent(LogEventLevel.Warning, "Failed processing [{0}] moved to [{1}]", null, directoryInfoToProcess.ToString(), movedTo);
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
                postDiscoveryScriptResult = await _postDiscoveryScript.ProcessAsync(fileSystemDirectoryInfo, cancellationToken).ConfigureAwait(false);
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

        LogAndRaiseEvent(LogEventLevel.Debug, "Processing Complete!");

        return new OperationResult<DirectoryProcessorResult>(processingMessages)
        {
            Errors = processingErrors.ToArray(),
            Data = new DirectoryProcessorResult
            {
                DurationInMs = Stopwatch.GetElapsedTime(startTicks).TotalMilliseconds,
                NewAlbumsCount = albumsIdsSeen.Distinct().Count(),
                NewArtistsCount = artistsIdsSeen.Distinct().Count(),
                NewSongsCount = songsIdsSeen.Distinct().Count(),
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
                Trace.WriteLine(e);
            }
        }

        OnProcessingEvent?.Invoke(this, exception?.ToString() ?? eventMessage);
    }

    //TODO make this use the Artist method to get images for an artist
    private static async Task<IEnumerable<ImageInfo>> FindImagesForArtist(Album album,
        ImageConvertor imageConvertor,
        IImageValidator imageValidator,
        short maxImageCount,
        bool doDeleteOriginal,
        CancellationToken cancellationToken = default)
    {
        var imageInfos = new List<ImageInfo>();
        var imageFiles = ImageHelper.ImageFilesInDirectory(album.OriginalDirectory.Path, SearchOption.TopDirectoryOnly).ToList();
        // If there are directories in the album directory that contains images include the images in that; we don't want to do AllDirectories as
        // there might be nested albums each with their own artist image directories.
        foreach (var dir in album.ImageDirectories())
        {
            imageFiles.AddRange(ImageHelper.ImageFilesInDirectory(dir.FullName, SearchOption.TopDirectoryOnly));
        }

        // Sometimes the album is in a directory with the parent holding an image artist that is not a discography folder 
        var parents = album.OriginalDirectory.GetParents().ToArray();
        var lookAtParentDirectoriesCount = parents.Length < 2 ? parents.Length : 2;
        for (var i = 0; i < lookAtParentDirectoriesCount; i++)
        {
            imageFiles.AddRange(ImageHelper.ImageFilesInDirectory(parents[i].FullName(), SearchOption.TopDirectoryOnly));
        }

        var index = 1;
        foreach (var imageFile in imageFiles.Order())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var fileInfo = new FileInfo(imageFile);
            if (ImageHelper.IsArtistImage(fileInfo) || ImageHelper.IsArtistSecondaryImage(fileInfo))
            {
                // Move the image if not in the album directory
                if (fileInfo.DirectoryName != album.Directory.FullName())
                {
                    var imageFileName = album.Directory.GetNextFileNameForType(maxImageCount, Data.Models.Artist.ImageType).Item1;
                    File.Copy(fileInfo.FullName, imageFileName, true);
                    if (doDeleteOriginal)
                    {
                        File.Delete(fileInfo.FullName);
                    }

                    fileInfo = new FileInfo(imageFileName);
                }

                if (!(await imageValidator.ValidateImage(fileInfo, ImageHelper.IsArtistImage(fileInfo) ? PictureIdentifier.Artist : PictureIdentifier.ArtistSecondary, cancellationToken).ConfigureAwait(false)).Data.IsValid)
                {
                    // Try converting (resizing and padding if needed) image and then revalidate
                    await imageConvertor.ProcessFileAsync(fileInfo.ToDirectorySystemInfo(), fileInfo.ToFileSystemInfo(), cancellationToken).ConfigureAwait(false);
                    if (!(await imageValidator.ValidateImage(fileInfo, ImageHelper.IsAlbumImage(fileInfo) ? PictureIdentifier.Front : PictureIdentifier.SecondaryFront, cancellationToken)).Data.IsValid)
                    {
                        continue;
                    }
                }

                var pictureIdentifier = PictureIdentifier.Band;
                if (ImageHelper.IsArtistSecondaryImage(fileInfo))
                {
                    pictureIdentifier = PictureIdentifier.BandSecondary;
                }

                var imageInfo = await Image.LoadAsync(fileInfo.FullName, cancellationToken).ConfigureAwait(false);
                var fileInfoFileSystemInfo = fileInfo.ToFileSystemInfo();
                imageInfos.Add(new ImageInfo
                {
                    CrcHash = Crc32.Calculate(fileInfo),
                    FileInfo = new FileSystemFileInfo
                    {
                        Name = $"{ImageInfo.ImageFilePrefix}{index.ToStringPadLeft(maxImageCount)}-{pictureIdentifier}.jpg",
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

        return imageInfos;
    }

    private static async Task<IEnumerable<ImageInfo>> FindImagesForAlbum(Album album, ImageConvertor imageConvertor, IImageValidator imageValidator, short maxNumberOfImagesLength, CancellationToken cancellationToken = default)
    {
        var imageInfos = new List<ImageInfo>();
        var imageFiles = ImageHelper.ImageFilesInDirectory(album.OriginalDirectory.Path, SearchOption.TopDirectoryOnly).ToList();
        // If there are directories in the album directory that contains images include the images in that; we don't want to do AllDirectories as there might be nested albums each with their own image directories.
        foreach (var dir in album.ImageDirectories())
        {
            imageFiles.AddRange(ImageHelper.ImageFilesInDirectory(dir.FullName, SearchOption.TopDirectoryOnly).Select(x => $"{dir.Name}-{x}"));
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
                    var newFileInfoName = Path.Combine(album.Directory.FullName(), $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}-{fileInfo.Directory!.Name}{Path.GetExtension(fileInfo.Name)}");
                    File.Move(fileInfo.FullName, newFileInfoName);
                    fileInfo = new FileInfo(newFileInfoName);
                }

                if (!(await imageValidator.ValidateImage(fileInfo, ImageHelper.IsAlbumImage(fileInfo) ? PictureIdentifier.Front : PictureIdentifier.SecondaryFront, cancellationToken).ConfigureAwait(false)).Data.IsValid)
                {
                    // Try converting (resizing and padding if needed) image and then revalidate
                    await imageConvertor.ProcessFileAsync(fileInfo.ToDirectorySystemInfo(), fileInfo.ToFileSystemInfo(), cancellationToken).ConfigureAwait(false);
                    if (!(await imageValidator.ValidateImage(fileInfo, ImageHelper.IsAlbumImage(fileInfo) ? PictureIdentifier.Front : PictureIdentifier.SecondaryFront, cancellationToken).ConfigureAwait(false)).Data.IsValid)
                    {
                        continue;
                    }
                }

                var fileNameNormalized = (fileInfo.Name.ToNormalizedString() ?? fileInfo.Name).Replace("AND", string.Empty);
                var albumNameNormalized = album.AlbumTitle().ToNormalizedString() ?? album.AlbumTitle() ?? string.Empty;
                var isAlbumImage = fileNameNormalized.Contains(albumNameNormalized, StringComparison.OrdinalIgnoreCase);
                if (isAlbumImage ||
                    ImageHelper.IsAlbumImage(fileInfo) ||
                    ImageHelper.IsAlbumSecondaryImage(fileInfo))
                {
                    var pictureIdentifier = PictureIdentifier.Front;
                    if (ImageHelper.IsAlbumSecondaryImage(fileInfo))
                    {
                        pictureIdentifier = PictureIdentifier.SecondaryFront;
                    }

                    var imageInfo = await Image.LoadAsync(fileInfo.FullName, cancellationToken).ConfigureAwait(false);
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

        return imageInfos;
    }
}
