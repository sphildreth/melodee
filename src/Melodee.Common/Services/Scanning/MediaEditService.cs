using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Plugins.MetaData.Song;
using Melodee.Common.Plugins.Processor;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Plugins.Validation.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Extensions;
using Melodee.Common.Services.Interfaces;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SixLabors.ImageSharp;
using ImageInfo = Melodee.Common.Models.ImageInfo;

namespace Melodee.Common.Services.Scanning;

/// <summary>
///     Service that edits media metadata.
/// </summary>
public sealed class MediaEditService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    LibraryService libraryService,
    AlbumDiscoveryService albumDiscoveryService,
    ISerializer serializer,
    IHttpClientFactory httpClientFactory) : ServiceBase(logger, cacheManager, contextFactory)
{
    private IAlbumValidator _albumValidator = new AlbumValidator(new MelodeeConfiguration([]));
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);
    private string _directoryLibrary = null!;

    private ISongPlugin _editSongPlugin = new NullSongPlugin();
    private ImageConvertor _imageConvertor = new(new MelodeeConfiguration([]));
    private IImageValidator _imageValidator = new ImageValidator(new MelodeeConfiguration([]));
    private bool _initialized;

    public async Task InitializeAsync(IMelodeeConfiguration? configuration = null, CancellationToken token = default)
    {
        _configuration = configuration ?? await configurationFactory.GetConfigurationAsync(token).ConfigureAwait(false);
        _albumValidator = new AlbumValidator(_configuration);
        _imageValidator = new ImageValidator(_configuration);
        _imageConvertor = new ImageConvertor(_configuration);
        _editSongPlugin = new AtlMetaTag(new MetaTagsProcessor(_configuration, serializer), _imageConvertor, _imageValidator, _configuration);

        // TODO this needs to support multiple storage libraries
        _directoryLibrary = (await libraryService.GetStorageLibrariesAsync(token)).Data.OrderBy(x => x.SortOrder).First().Path;

        await albumDiscoveryService.InitializeAsync(configuration, token).ConfigureAwait(false);

        _initialized = true;
    }

    private void CheckInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Media edit service is not initialized.");
        }
    }

    public async Task<OperationResult<Album?>> SaveImageUrlAsCoverAsync(FileSystemDirectoryInfo directoryInfo, Guid albumId, string imageUrl, bool deleteAllCoverImages, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, albumId, cancellationToken);
        if (album.Directory != null)
        {
            try
            {
                var imageBytes = await httpClientFactory.BytesForImageUrlAsync(_configuration.GetValue<string?>(SettingRegistry.SearchEngineUserAgent) ?? string.Empty, imageUrl, cancellationToken);
                if (imageBytes != null)
                {
                    var albumImages = album.Images?.ToList() ?? [];

                    if (deleteAllCoverImages)
                    {
                        albumImages.RemoveAll(x => x.PictureIdentifier is (PictureIdentifier.Front or PictureIdentifier.SecondaryFront or PictureIdentifier.NotSet));
                        var filesInAlbumDirectory = album.Directory.FileInfosForExtension("jpg").ToArray();
                        if (filesInAlbumDirectory.Any())
                        {
                            foreach (var fileInAlbumDirectory in filesInAlbumDirectory)
                            {
                                if (fileInAlbumDirectory.Name.Contains(PictureIdentifier.Front.ToString(), StringComparison.OrdinalIgnoreCase) ||
                                    fileInAlbumDirectory.Name.Contains(PictureIdentifier.SecondaryFront.ToString(), StringComparison.OrdinalIgnoreCase) ||
                                    fileInAlbumDirectory.Name.Contains(PictureIdentifier.NotSet.ToString(), StringComparison.OrdinalIgnoreCase))
                                {
                                    fileInAlbumDirectory.Delete();
                                }
                            }
                        }
                    }

                    var imageConvertor = new ImageConvertor(_configuration);
                    var numberOfExistingFrontImages = album.Images?.Count(x => x.PictureIdentifier == PictureIdentifier.Front) ?? 0;
                    var tempFilename = Path.Combine(album.Directory.FullName(), deleteAllCoverImages ? "01-Front.image" : $"{numberOfExistingFrontImages + 1}-Front.image");
                    var tempFileInfo = new FileInfo(tempFilename).ToFileSystemInfo();
                    await File.WriteAllBytesAsync(tempFileInfo.FullName(directoryInfo), imageBytes, cancellationToken);
                    var imageConversionResult = await imageConvertor.ProcessFileAsync(
                        album.Directory,
                        tempFileInfo,
                        cancellationToken);
                    var imageFileInfo = imageConversionResult.Data;
                    imageBytes = await File.ReadAllBytesAsync(imageFileInfo.FullName(directoryInfo), cancellationToken);

                    var existingCoverImage = albumImages.FirstOrDefault(x => x.PictureIdentifier == PictureIdentifier.Front);
                    if (existingCoverImage?.FileInfo != null)
                    {
                        File.Delete(existingCoverImage.FileInfo.FullName(directoryInfo));
                        albumImages.RemoveAll(x => x == existingCoverImage);
                    }

                    var imageInfo = Image.Load(imageBytes);
                    albumImages.Add(new ImageInfo
                    {
                        CrcHash = Crc32.Calculate(imageFileInfo.ToFileInfo(directoryInfo)),
                        FileInfo = imageFileInfo,
                        Height = imageInfo.Height,
                        PictureIdentifier = PictureIdentifier.Front,
                        SortOrder = deleteAllCoverImages ? 1 : albumImages.Max(x => x.SortOrder) + 1,
                        Width = imageInfo.Width
                    });
                    album.Images = albumImages.ToArray();
                }

                var newAlbumId = await SaveAlbum(directoryInfo, album, cancellationToken);
                return new OperationResult<Album?>
                {
                    Data = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, newAlbumId, cancellationToken)
                };
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error attempting to download mage Url [{Url}] for album [{Album}]", imageUrl, album);
            }
        }

        return new OperationResult<Album?>("An error has occured. OH NOES!")
        {
            Data = null
        };
    }

    public async Task<OperationResult<AlbumValidationResult>> DoMagic(FileSystemDirectoryInfo directoryInfo, Guid albumId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        if (!SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicEnabled]))
        {
            return new OperationResult<AlbumValidationResult>
            {
                Data = new AlbumValidationResult(AlbumStatus.NotSet, AlbumNeedsAttentionReasons.NotSet)
            };
        }

        var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, albumId, cancellationToken);
        var albumValidResult = _albumValidator.ValidateAlbum(album);
        album.ValidationMessages = albumValidResult.Data.Messages ?? [];
        album.Status = albumValidResult.Data.AlbumStatus;
        album.StatusReasons = albumValidResult.Data.AlbumStatusReasons;
        if (albumValidResult.Data.IsValid)
        {
            return new OperationResult<AlbumValidationResult>
            {
                Data = new AlbumValidationResult(AlbumStatus.Invalid, AlbumNeedsAttentionReasons.NotSet)
            };
        }

        if (!(album.Directory?.Exists() ?? false))
        {
            Logger.Warning("Album directory is invalid.");
            return new OperationResult<AlbumValidationResult>
            {
                Data = new AlbumValidationResult(AlbumStatus.Invalid, AlbumNeedsAttentionReasons.AlbumCannotBeLoaded)
            };
        }

        if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicDoRenumberSongs]))
        {
            albumId = (await RenumberSongs(directoryInfo, albumId, cancellationToken)).Data.Item2;
        }

        if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicDoRemoveFeaturingArtistFromSongArtist]))
        {
            albumId = (await RemoveFeaturingArtistsFromSongsArtist(directoryInfo, albumId, cancellationToken)).Data.Item2;
        }

        if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicDoRemoveFeaturingArtistFromSongTitle]))
        {
            albumId = (await RemoveFeaturingArtistsFromSongTitle(directoryInfo, albumId, cancellationToken)).Data.Item2;
        }

        if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicDoReplaceSongsArtistSeparators]))
        {
            albumId = (await ReplaceAllSongArtistSeparators(directoryInfo, albumId, cancellationToken)).Data.Item2;
        }

        if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicDoSetYearToCurrentIfInvalid]))
        {
            albumId = (await SetYearToCurrent(directoryInfo, albumId, cancellationToken)).Data.Item2;
        }

        if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicDoRemoveUnwantedTextFromAlbumTitle]))
        {
            albumId = (await RemoveUnwantedTextFromAlbumTitle(directoryInfo, albumId, cancellationToken)).Data.Item2;
        }

        if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicDoRemoveUnwantedTextFromSongTitles]))
        {
            await RemoveUnwantedTextFromSongTitles(directoryInfo, albumId, cancellationToken);
        }

        var validationResult = _albumValidator.ValidateAlbum(album);
        album.Status = validationResult.Data.AlbumStatus;
        album.StatusReasons = validationResult.Data.AlbumStatusReasons;
        album.Modified = DateTimeOffset.UtcNow;
        await SaveAlbum(directoryInfo, album, cancellationToken);
        return validationResult;
    }

    private async Task<Guid> SaveAlbum(FileSystemDirectoryInfo directoryInfo, Album album, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var serialized = serializer.Serialize(album);
        var albumStagingDirInfo = new DirectoryInfo(directoryInfo.FullName());
        var jsonName = Path.Combine(albumStagingDirInfo.FullName, album.ToMelodeeJsonName(_configuration, true));
        await File.WriteAllTextAsync(jsonName, serialized, cancellationToken);
        return album.Id;
    }

    public async Task<OperationResult<(bool, Guid)>> RemoveUnwantedTextFromAlbumTitle(FileSystemDirectoryInfo directoryInfo, Guid albumId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = false;
        var newAlbumId = albumId;
        try
        {
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, albumId, cancellationToken);
            if (album.Songs?.Count() > 0)
            {
                var title = album.AlbumTitle();
                var newTitle = AlbumValidator.RemoveUnwantedTextFromAlbumTitle(title);
                if (!string.Equals(title, newTitle, StringComparison.OrdinalIgnoreCase))
                {
                    album.SetTagValue(MetaTagIdentifier.Album, newTitle);
                    foreach (var song in album.Songs)
                    {
                        album.SetSongTagValue(song.Id, MetaTagIdentifier.Album, newTitle);
                        await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                    }

                    newAlbumId = await SaveAlbum(directoryInfo, album, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Remove unwanted text from album title.");
        }

        return new OperationResult<(bool, Guid)>
        {
            Data = (result, newAlbumId)
        };
    }

    public async Task<OperationResult<(bool, Guid)>> RemoveUnwantedTextFromSongTitles(FileSystemDirectoryInfo directoryInfo, Guid albumId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = false;
        var newAlbumId = albumId;
        try
        {
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, albumId, cancellationToken);
            if (album.Songs?.Count() > 0)
            {
                var modified = false;
                foreach (var song in album.Songs)
                {
                    var title = song.Title();
                    var newTitle = AlbumValidator.RemoveUnwantedTextFromSongTitle(title);
                    if (!string.Equals(title, newTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        album.SetSongTagValue(song.Id, MetaTagIdentifier.Title, newTitle);
                        await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                        modified = true;
                    }
                }

                if (modified)
                {
                    newAlbumId = await SaveAlbum(directoryInfo, album, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Removing unwanted text from song titles.");
        }

        return new OperationResult<(bool, Guid)>
        {
            Data = (result, newAlbumId)
        };
    }


    public async Task<OperationResult<(bool, Guid)>> RemoveFeaturingArtistsFromSongTitle(FileSystemDirectoryInfo directoryInfo, Guid albumId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = false;
        var newAlbumId = albumId;
        try
        {
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, albumId, cancellationToken);
            if (album.Songs?.Count() > 0)
            {
                foreach (var song in album.Songs)
                {
                    var songTitle = song.Title();
                    if (songTitle != null && AlbumValidator.StringHasFeaturingFragments(songTitle))
                    {
                        var matches = AlbumValidator.HasFeatureFragmentsRegex.Match(songTitle);
                        var newSongTitle = AlbumValidator.ReplaceSongArtistSeparators(AlbumValidator.HasFeatureFragmentsRegex.Replace(songTitle.Substring(matches.Index), string.Empty).CleanString());
                        newSongTitle = newSongTitle?.TrimEnd(']', ')').Replace("\"", "'");
                        album.SetSongTagValue(song.Id, MetaTagIdentifier.Title, newSongTitle);
                        await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                    }
                }

                newAlbumId = await SaveAlbum(directoryInfo, album, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Remove featuring Artists from Songs Artist.");
        }

        return new OperationResult<(bool, Guid)>
        {
            Data = (result, newAlbumId)
        };
    }

    public async Task<OperationResult<(bool, Guid)>> RemoveFeaturingArtistsFromSongsArtist(FileSystemDirectoryInfo directoryInfo, Guid albumId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = false;
        var newAlbumId = albumId;
        try
        {
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, albumId, cancellationToken);
            foreach (var song in album.Songs?.Where(t => t.SongArtist().Nullify() != null) ?? [])
            {
                var songArtist = song.SongArtist();
                if (songArtist != null && AlbumValidator.StringHasFeaturingFragments(songArtist))
                {
                    var matches = AlbumValidator.HasFeatureFragmentsRegex.Match(songArtist);
                    var newSongArtist = AlbumValidator.ReplaceSongArtistSeparators(AlbumValidator.HasFeatureFragmentsRegex.Replace(songArtist.Substring(matches.Index), string.Empty).CleanString());
                    newSongArtist = newSongArtist?.TrimEnd(']', ')').Replace("\"", "'");
                    album.SetSongTagValue(song.Id, MetaTagIdentifier.Artist, newSongArtist);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                }
            }

            newAlbumId = await SaveAlbum(directoryInfo, album, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Remove featuring Artists from Songs Artist.");
        }

        return new OperationResult<(bool, Guid)>
        {
            Data = (result, newAlbumId)
        };
    }

    public async Task<OperationResult<bool>> PromoteSongArtist(FileSystemDirectoryInfo directoryInfo, Guid albumId, Guid selectedSongId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = false;
        try
        {
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, albumId, cancellationToken);
            var artistToPromote = album.Songs?.FirstOrDefault(x => x.Id == selectedSongId)?.SongArtist();
            if (artistToPromote.Nullify() != null)
            {
                album.SetTagValue(MetaTagIdentifier.AlbumArtist, artistToPromote);
                foreach (var song in album.Songs!)
                {
                    album.RemoveSongTagValue(song.Id, MetaTagIdentifier.Artist);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                }

                await SaveAlbum(directoryInfo, album, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Promote Song Artist to Album Artist.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<(bool, Guid)>> SetYearToCurrent(FileSystemDirectoryInfo directoryInfo, Guid albumId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = false;
        var newAlbumId = albumId;
        try
        {
            var year = DateTime.Now.Year;
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, albumId, cancellationToken);
            var albumValidResult = _albumValidator.ValidateAlbum(album);
            if (albumValidResult.Data.IsValid && albumValidResult.Data.AlbumStatusReasons.HasFlag(AlbumNeedsAttentionReasons.HasInvalidYear))
            {
                album.SetTagValue(MetaTagIdentifier.OrigAlbumYear, year);
                foreach (var song in album.Songs ?? [])
                {
                    album.SetSongTagValue(song.Id, MetaTagIdentifier.OrigAlbumYear, year);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                }

                newAlbumId = await SaveAlbum(directoryInfo, album, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Set Year to current year.");
        }

        return new OperationResult<(bool, Guid)>
        {
            Data = (result, newAlbumId)
        };
    }

    public async Task<OperationResult<bool>> ReplaceGivenTextFromSongTitles(FileSystemDirectoryInfo directoryInfo, Guid albumId, string textToRemove, string? textToReplaceWith, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = false;
        try
        {
            var modified = false;
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, albumId, cancellationToken);
            foreach (var song in album.Songs!)
            {
                var originalTitle = song.Title() ?? string.Empty;
                var newTitle = song.Title()!.Replace(originalTitle, textToReplaceWith ?? string.Empty).Trim();
                if (!string.Equals(originalTitle, newTitle, StringComparison.OrdinalIgnoreCase))
                {
                    album.SetSongTagValue(song.Id, MetaTagIdentifier.Title, newTitle);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                    modified = true;
                }
            }

            if (modified)
            {
                await SaveAlbum(directoryInfo, album, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Replace text with given text for all Song Titles.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> RemoveAllSongArtists(FileSystemDirectoryInfo directoryInfo, Guid[] albumIds, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        if (albumIds.Length == 0)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            foreach (var selectedAlbumId in albumIds)
            {
                var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, selectedAlbumId, cancellationToken);
                foreach (var song in album.Songs!)
                {
                    album.RemoveSongTagValue(song.Id, MetaTagIdentifier.AlbumArtist);
                    album.RemoveSongTagValue(song.Id, MetaTagIdentifier.Artist);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                }

                await SaveAlbum(directoryInfo, album, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Remove all Song Artists.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<(bool, Guid)>> ReplaceAllSongArtistSeparators(FileSystemDirectoryInfo directoryInfo, Guid albumId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = false;
        var newAlbumId = albumId;
        try
        {
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, albumId, cancellationToken);
            foreach (var song in album.Songs?.Where(x => x.SongArtist().Nullify() != null) ?? [])
            {
                var oldSongArtist = song.SongArtist();
                var newSongArtist = MetaTagsProcessor.ReplaceSongArtistSeparators(oldSongArtist);
                if (!string.Equals(oldSongArtist, newSongArtist, StringComparison.OrdinalIgnoreCase))
                {
                    album.SetSongTagValue(song.Id, MetaTagIdentifier.AlbumArtist, null);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                }
            }

            newAlbumId = await SaveAlbum(directoryInfo, album, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Replace all Song Artist separators.");
        }

        return new OperationResult<(bool, Guid)>
        {
            Data = (result, newAlbumId)
        };
    }

    public async Task<OperationResult<(bool, Guid)>> RenumberSongs(FileSystemDirectoryInfo directoryInfo, Guid albumId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = false;
        var newAlbumId = albumId;
        try
        {
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, albumId, cancellationToken);
            var numberOfMedias = album.MediaCountValue();
            var mediaLooper = 0;
            while (mediaLooper <= numberOfMedias)
            {
                var looper = mediaLooper;
                foreach (var dd in album.Songs?.Where(x => x.MediaNumber() == looper).Select((x, i) => new { x, i = i + 1 }) ?? [])
                {
                    album.SetSongTagValue(dd.x.Id, MetaTagIdentifier.TrackNumber, dd.i);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, dd.x, cancellationToken);
                }

                mediaLooper++;
            }

            newAlbumId = await SaveAlbum(directoryInfo, album, cancellationToken);
            result = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Renumbering Songs.");
        }

        return new OperationResult<(bool, Guid)>
        {
            Data = (result, newAlbumId)
        };
    }

    public async Task<OperationResult<bool>> DeleteAllImagesForAlbums(FileSystemDirectoryInfo directoryInfo, Guid[] albumIds, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        if (albumIds.Length == 0)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            foreach (var selectedAlbumId in albumIds)
            {
                var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, selectedAlbumId, cancellationToken);
                var albumStagingDirInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName(), album.ToDirectoryName()));
                album.Images = [];
                var serialized = serializer.Serialize(album);
                await File.WriteAllTextAsync(Path.Combine(albumStagingDirInfo.FullName, album.ToMelodeeJsonName(_configuration, true)), serialized, cancellationToken);
                foreach (var imageFile in ImageHelper.ImageFilesInDirectory(albumStagingDirInfo.FullName, SearchOption.AllDirectories))
                {
                    File.Delete(imageFile);
                }

                foreach (var song in album.Songs!)
                {
                    song.Images = [];
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                }

                await SaveAlbum(directoryInfo, album, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Deleting all images for Albums.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> RemoveArtistFromSongArtists(FileSystemDirectoryInfo directoryInfo, Guid[] albumIds, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        if (albumIds.Length == 0)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            foreach (var selectedAlbumId in albumIds)
            {
                var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, selectedAlbumId, cancellationToken);
                foreach (var song in album.Songs!)
                {
                    album.SetSongTagValue(song.Id, MetaTagIdentifier.AlbumArtist, null);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                }

                await SaveAlbum(directoryInfo, album, cancellationToken);
            }

            result = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Remove Artist from Song Artists.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> SetAlbumsStatusToReviewed(FileSystemDirectoryInfo directoryInfo, Guid[] albumIds, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        if (albumIds.Length == 0)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            foreach (var selectedAlbumId in albumIds)
            {
                var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, selectedAlbumId, cancellationToken);
                album.Status = AlbumStatus.Ok;
                await SaveAlbum(directoryInfo, album, cancellationToken);
            }

            result = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Set Albums status to reviewed.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> MoveAlbumsToLibraryAsync(FileSystemDirectoryInfo directoryInfo, Guid[] albumIds, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        if (albumIds.Length == 0)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            foreach (var selectedAlbumId in albumIds)
            {
                var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, selectedAlbumId, cancellationToken);
                var albumStagingDirInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName(), album.ToDirectoryName()));
                var albumLibraryDirInfo = new DirectoryInfo(Path.Combine(_directoryLibrary, album.ToDirectoryName()));
                albumLibraryDirInfo.ToDirectorySystemInfo().MoveToDirectory(albumLibraryDirInfo.FullName);
            }

            directoryInfo.DeleteAllEmptyDirectories();
            result = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Moving Albums To library.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> SaveMelodeeAlbum(Album album, CancellationToken cancellationToken = default)
    {
        album.Modified = DateTimeOffset.UtcNow;
        var albumValidResult = _albumValidator.ValidateAlbum(album);
        album.ValidationMessages = albumValidResult.Data.Messages ?? [];
        album.Status = albumValidResult.Data.AlbumStatus;
        album.StatusReasons = albumValidResult.Data.AlbumStatusReasons;
        await SaveAlbum(album.Directory, album, cancellationToken);

        return new OperationResult<bool>
        {
            Data = album.Status == AlbumStatus.Ok
        };
    }
}
