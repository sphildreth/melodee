using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Plugins.Conversion.Image;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Validation;
using Melodee.Plugins.Validation.Models;
using Melodee.Services.Extensions;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SixLabors.ImageSharp;
using ImageInfo = Melodee.Common.Models.ImageInfo;

namespace Melodee.Services.Scanning;

/// <summary>
///     Service that edits media metadata.
/// </summary>
public sealed class MediaEditService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ISettingService settingService,
    ILibraryService libraryService,
    AlbumDiscoveryService albumDiscoveryService,
    ISerializer serializer,
    IHttpClientFactory httpClientFactory) : ServiceBase(logger, cacheManager, contextFactory)
{
    private IAlbumValidator _albumValidator = new AlbumValidator(new MelodeeConfiguration([]));
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);
    private string _directoryLibrary = null!;
    
    private ISongPlugin _editSongPlugin = new NullSongPlugin();
    private bool _initialized;

    public async Task InitializeAsync(IMelodeeConfiguration? configuration = null, CancellationToken token = default)
    {
        _configuration = configuration ?? await settingService.GetMelodeeConfigurationAsync(token).ConfigureAwait(false);
        _albumValidator = new AlbumValidator(_configuration);
        _editSongPlugin = new AtlMetaTag(new MetaTagsProcessor(_configuration, serializer), _configuration);

        _directoryLibrary = (await libraryService.GetLibraryAsync(token)).Data.Path;

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

    public async Task<OperationResult<Album?>> SaveImageUrlAsCoverAsync(FileSystemDirectoryInfo directoryInfo, long albumId, string imageUrl, bool deleteAllCoverImages, CancellationToken cancellationToken = default)
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
                    var tempFilename = Path.Combine(album.Directory.FullName(), deleteAllCoverImages ? $"01-Front.image" : $"{numberOfExistingFrontImages + 1}-Front.image");
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
                        Width = imageInfo.Width,
                    });
                    album.Images = albumImages.ToArray();
                }
                var newAlbumId = await SaveAlbum(directoryInfo, album, cancellationToken);
                return new OperationResult<Album?>()
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

    public async Task<OperationResult<ValidationResult>> DoMagic(FileSystemDirectoryInfo directoryInfo, long albumId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        if (!SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicEnabled]))
        {
            return new OperationResult<ValidationResult>
            {
                Data = new ValidationResult
                {
                    AlbumStatus = AlbumStatus.NotSet
                }
            };
        }

        var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, albumId, cancellationToken);
        var albumValidResult = album.IsValid(_configuration.Configuration);
        if (!albumValidResult.Item1)
        {
            Logger.Warning($"Album [{album}] is invalid [{albumValidResult.Item2}]");
            return new OperationResult<ValidationResult>
            {
                Data = new ValidationResult
                {
                    AlbumStatus = AlbumStatus.Invalid
                }
            };
        }

        if (!(album.Directory?.Exists() ?? false))
        {
            Logger.Warning("Album directory is invalid.");
            return new OperationResult<ValidationResult>
            {
                Data = new ValidationResult
                {
                    AlbumStatus = AlbumStatus.Invalid
                }
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
        album.Modified = DateTimeOffset.UtcNow;
        await SaveAlbum(directoryInfo, album, cancellationToken);
        return validationResult;
    }

    private async Task<long> SaveAlbum(FileSystemDirectoryInfo directoryInfo, Album album, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var serialized = serializer.Serialize(album);
        var albumStagingDirInfo = new DirectoryInfo(directoryInfo.FullName());
        var jsonName = Path.Combine(albumStagingDirInfo.FullName, album.ToMelodeeJsonName(true));
        await File.WriteAllTextAsync(jsonName, serialized, cancellationToken);
        return album.UniqueId;
    }

    public async Task<OperationResult<(bool, long)>> RemoveUnwantedTextFromAlbumTitle(FileSystemDirectoryInfo directoryInfo, long albumId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        if (albumId < 1)
        {
            return new OperationResult<(bool, long)>
            {
                Data = (false, albumId)
            };
        }

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
                        album.SetSongTagValue(song.SongId, MetaTagIdentifier.Album, newTitle);
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

        return new OperationResult<(bool, long)>
        {
            Data = (result,newAlbumId)
        };
    }

    public async Task<OperationResult<(bool, long)>> RemoveUnwantedTextFromSongTitles(FileSystemDirectoryInfo directoryInfo, long albumId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        if (albumId < 1)
        {
            return new OperationResult<(bool, long)>
            {
                Data = (false, albumId)
            };
        }

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
                        album.SetSongTagValue(song.SongId, MetaTagIdentifier.Title, newTitle);
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

        return new OperationResult<(bool, long)>
        {
            Data = (result, newAlbumId)
        };
    }


    public async Task<OperationResult<(bool, long)>> RemoveFeaturingArtistsFromSongTitle(FileSystemDirectoryInfo directoryInfo, long albumId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        if (albumId < 1)
        {
            return new OperationResult<(bool, long)>
            {
                Data = (false, albumId)
            };
        }

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
                        album.SetSongTagValue(song.SongId, MetaTagIdentifier.Title, newSongTitle);
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

        return new OperationResult<(bool, long)>
        {
            Data = (result, newAlbumId)
        };
    }

    public async Task<OperationResult<(bool, long)>> RemoveFeaturingArtistsFromSongsArtist(FileSystemDirectoryInfo directoryInfo, long albumId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        if (albumId < 1)
        {
            return new OperationResult<(bool, long)>
            {
                Data = (false, albumId)
            };
        }

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
                    album.SetSongTagValue(song.SongId, MetaTagIdentifier.Artist, newSongArtist);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                }
            }

            newAlbumId = await SaveAlbum(directoryInfo, album, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Remove featuring Artists from Songs Artist.");
        }

        return new OperationResult<(bool, long)>
        {
            Data = (result, newAlbumId)
        };
    }

    public async Task<OperationResult<bool>> PromoteSongArtist(FileSystemDirectoryInfo directoryInfo, long albumId, long selectedSongId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        if (albumId < 1)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, albumId, cancellationToken);
            var artistToPromote = album.Songs?.FirstOrDefault(x => x.SongId == selectedSongId)?.SongArtist();
            if (artistToPromote.Nullify() != null)
            {
                album.SetTagValue(MetaTagIdentifier.AlbumArtist, artistToPromote);
                foreach (var song in album.Songs!)
                {
                    album.RemoveSongTagValue(song.SongId, MetaTagIdentifier.Artist);
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

    public async Task<OperationResult<(bool, long)>> SetYearToCurrent(FileSystemDirectoryInfo directoryInfo, long albumId, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = false;
        var newAlbumId = albumId;
        try
        {
            var year = DateTime.Now.Year;
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(directoryInfo, albumId, cancellationToken);
            if (album.IsValid(_configuration.Configuration).Item1)
            {
                album.SetTagValue(MetaTagIdentifier.OrigAlbumYear, year);
                foreach (var song in album.Songs ?? [])
                {
                    album.SetSongTagValue(song.SongId, MetaTagIdentifier.OrigAlbumYear, year);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                }

                newAlbumId = await SaveAlbum(directoryInfo, album, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Set Year to current year.");
        }

        return new OperationResult<(bool, long)>
        {
            Data = (result, newAlbumId)
        };
    }

    public async Task<OperationResult<bool>> ReplaceGivenTextFromSongTitles(FileSystemDirectoryInfo directoryInfo, long albumId, string textToRemove, string? textToReplaceWith, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        if (albumId < 1 || textToRemove.Nullify() == null)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

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
                    album.SetSongTagValue(song.SongId, MetaTagIdentifier.Title, newTitle);
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

    public async Task<OperationResult<bool>> RemoveAllSongArtists(FileSystemDirectoryInfo directoryInfo, long[] albumIds, CancellationToken cancellationToken = default)
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
                    album.RemoveSongTagValue(song.SongId, MetaTagIdentifier.AlbumArtist);
                    album.RemoveSongTagValue(song.SongId, MetaTagIdentifier.Artist);
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

    public async Task<OperationResult<(bool, long)>> ReplaceAllSongArtistSeparators(FileSystemDirectoryInfo directoryInfo, long albumId, CancellationToken cancellationToken = default)
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
                    album.SetSongTagValue(song.SongId, MetaTagIdentifier.AlbumArtist, null);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                }
            }

            newAlbumId = await SaveAlbum(directoryInfo, album, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Replace all Song Artist separators.");
        }

        return new OperationResult<(bool, long)>
        {
            Data = (result, newAlbumId)
        };
    }

    public async Task<OperationResult<(bool, long)>> RenumberSongs(FileSystemDirectoryInfo directoryInfo, long albumId, CancellationToken cancellationToken = default)
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
                    album.SetSongTagValue(dd.x.SongId, MetaTagIdentifier.TrackNumber, dd.i);
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

        return new OperationResult<(bool, long)>
        {
            Data = (result, newAlbumId)
            
        };
    }

    public async Task<OperationResult<bool>> DeleteAllImagesForAlbums(FileSystemDirectoryInfo directoryInfo, long[] albumIds, CancellationToken cancellationToken = default)
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
                await File.WriteAllTextAsync(Path.Combine(albumStagingDirInfo.FullName, album.ToMelodeeJsonName(true)), serialized, cancellationToken);
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

    public async Task<OperationResult<bool>> RemoveArtistFromSongArtists(FileSystemDirectoryInfo directoryInfo, long[] albumIds, CancellationToken cancellationToken = default)
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
                    album.SetSongTagValue(song.SongId, MetaTagIdentifier.AlbumArtist, null);
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

    public async Task<OperationResult<bool>> SetAlbumsStatusToReviewed(FileSystemDirectoryInfo directoryInfo, long[] albumIds, CancellationToken cancellationToken = default)
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

    public async Task<OperationResult<bool>> DeleteAlbumsInStagingAsync(FileSystemDirectoryInfo directoryInfo, long[] albumIds, CancellationToken cancellationToken = default)
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
                try
                {
                    Directory.Delete(albumStagingDirInfo.FullName, true);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error deleting [{AlbumId}]", selectedAlbumId);
                }
            }

            result = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Deleting Albums from staging.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> MoveAlbumsToLibraryAsync(FileSystemDirectoryInfo directoryInfo, long[] albumIds, CancellationToken cancellationToken = default)
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
                MoveDirectory(albumStagingDirInfo.FullName, albumLibraryDirInfo.FullName, null);
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

    /// <summary>
    ///     This exists because in some systems where data is on one mapped drive it cannot be "Moved" to another mapped drive
    ///     ("Cross link" error), it must be copied and then deleted.
    /// </summary>
    public static void MoveDirectory(string toMove, string destination, string? dontMoveFileName)
    {
        if (!Directory.Exists(destination))
        {
            Directory.CreateDirectory(destination);
        }

        var files = Directory.GetFiles(toMove);
        var directories = Directory.GetDirectories(toMove);
        foreach (var file in files)
        {
            if (!string.Equals(Path.GetFileName(file), dontMoveFileName, StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
            }
        }

        foreach (var d in directories)
        {
            MoveDirectory(Path.Combine(toMove, Path.GetFileName(d)), Path.Combine(destination, Path.GetFileName(d)), dontMoveFileName);
        }

        Directory.Delete(toMove, true);
    }
}
