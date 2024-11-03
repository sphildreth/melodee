using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Validation;
using Melodee.Plugins.Validation.Models;
using Melodee.Services.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Melodee.Services.Scanning;

/// <summary>
/// Service that edits media metadata. 
/// </summary>
public sealed class MediaEditService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    SettingService settingService,
    LibraryService libraryService,
    AlbumDiscoveryService albumDiscoveryService,
    ISerializer serializer): ServiceBase(logger, cacheManager, contextFactory)
{
    private readonly LibraryService _libraryService = libraryService;
    private bool _initialized;
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);
    private IAlbumValidator _albumValidator = new AlbumValidator(new MelodeeConfiguration([]));
    private ISongPlugin _editSongPlugin = new NullSongPlugin();

    private string _directoryStaging = null!;
    private DirectoryInfo DirectoryStagingInfo => new DirectoryInfo(_directoryStaging);
    private FileSystemDirectoryInfo DirectoryStagingFileSystemDirectoryInfo => DirectoryStagingInfo.ToDirectorySystemInfo();
    private string _directoryLibrary= null!;
    
    public async Task InitializeAsync(IMelodeeConfiguration? configuration = null, CancellationToken token = default)
    {
        _configuration = configuration ?? await settingService.GetMelodeeConfigurationAsync(token).ConfigureAwait(false);
        _albumValidator = new AlbumValidator(_configuration);
        _editSongPlugin = new AtlMetaTag(new MetaTagsProcessor(_configuration, serializer), _configuration);
     
        _directoryLibrary = configuration?.GetValue<string?>(SettingRegistry.DirectoryLibrary) ?? (await _libraryService.GetLibraryAsync(token)).Data.Path;        
        _directoryStaging = configuration?.GetValue<string?>(SettingRegistry.DirectoryStaging) ?? (await _libraryService.GetStagingLibraryAsync(token)).Data.Path;
        
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
   

    public async Task<OperationResult<ValidationResult>> DoMagic(long albumId, CancellationToken cancellationToken = default)
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
        if ( SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicDoRenumberSongs]))
        {
            await RenumberSongs([albumId], cancellationToken);
        }
        if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicDoRemoveFeaturingArtistFromSongArtist]))
        {
            await RemoveFeaturingArtistsFromSongsArtist(albumId, cancellationToken);
        }
        if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicDoRemoveFeaturingArtistFromSongTitle]))
        {
            await RemoveFeaturingArtistsFromSongTitle(albumId, cancellationToken);
        }
        if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicDoReplaceSongsArtistSeparators]))
        {
            await ReplaceAllSongArtistSeparators([albumId], cancellationToken);
        }
        if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicDoSetYearToCurrentIfInvalid]))
        {
            await SetYearToCurrent([albumId], cancellationToken);
        }       
        if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicDoRemoveUnwantedTextFromAlbumTitle]))
        {
            await RemoveUnwantedTextFromAlbumTitle(albumId, cancellationToken);
        }
        var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, albumId, cancellationToken);
        var validationResult = _albumValidator.ValidateAlbum(album);
        album.Status = validationResult.Data.AlbumStatus;
        album.Modified = DateTimeOffset.UtcNow;
        await SaveAlbum(album, cancellationToken);
        return validationResult;
    }

    private async Task SaveAlbum(Album album, CancellationToken cancellationToken = default)
    {
        CheckInitialized();
        
        var serialized = serializer.Serialize(album);
        var albumDirectoryName = album.ToDirectoryName();
        if (albumDirectoryName.Nullify() != null)
        {
            var albumStagingDirInfo = new DirectoryInfo(Path.Combine(_directoryStaging, albumDirectoryName));
            var jsonName = Path.Combine(albumStagingDirInfo.FullName, album.ToMelodeeJsonName(true));
            await File.WriteAllTextAsync(jsonName, serialized, cancellationToken);
        }
        else
        {
            Log.Warning("[{Album}] has invalid Directory Name [{AlbumDirectoryName}]", album.ToString(), albumDirectoryName);
        }
    }

    public async Task<OperationResult<bool>> RemoveUnwantedTextFromAlbumTitle(long albumId, CancellationToken cancellationToken = default)
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
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, albumId, cancellationToken);
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
                    await SaveAlbum(album, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Remove featuring Artists from Songs Artist.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };        
    }
    
    public async Task<OperationResult<bool>> RemoveFeaturingArtistsFromSongTitle(long albumId, CancellationToken cancellationToken = default)
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
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, albumId, cancellationToken);
            if (album.Songs?.Count() > 0)
            {
                foreach (var song in album.Songs)
                {
                    var songTitle = song.Title();
                    if (songTitle != null && AlbumValidator.StringHasFeaturingFragments(songTitle))
                    {
                        var matches = AlbumValidator.HasFeatureFragmentsRegex.Match(songTitle);
                        string? newSongTitle = AlbumValidator.ReplaceSongArtistSeparators(AlbumValidator.HasFeatureFragmentsRegex.Replace(songTitle.Substring(matches.Index), string.Empty).CleanString());
                        newSongTitle = newSongTitle?.TrimEnd(']', ')').Replace("\"", "'");
                        album.SetSongTagValue(song.SongId, MetaTagIdentifier.Title, newSongTitle);
                        await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                    }
                }
                await SaveAlbum(album, cancellationToken);                
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Remove featuring Artists from Songs Artist.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }    

    public async Task<OperationResult<bool>> RemoveFeaturingArtistsFromSongsArtist(long albumId, CancellationToken cancellationToken = default)
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
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, albumId, cancellationToken);
            foreach (var song in album.Songs?.Where(t => t.SongArtist().Nullify() != null) ?? [])
            {
                var songArtist = song.SongArtist();
                if (songArtist != null && AlbumValidator.StringHasFeaturingFragments(songArtist))
                {
                    var matches = AlbumValidator.HasFeatureFragmentsRegex.Match(songArtist);
                    string? newSongArtist = AlbumValidator.ReplaceSongArtistSeparators(AlbumValidator.HasFeatureFragmentsRegex.Replace(songArtist.Substring(matches.Index), string.Empty).CleanString());
                    newSongArtist = newSongArtist?.TrimEnd(']', ')').Replace("\"", "'");
                    album.SetSongTagValue(song.SongId, MetaTagIdentifier.Artist, newSongArtist);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                }
            }
            await SaveAlbum(album, cancellationToken); 
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Remove featuring Artists from Songs Artist.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> PromoteSongArtist(long albumId, long selectedSongId, CancellationToken cancellationToken = default)
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
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, albumId, cancellationToken);
            var artistToPromote = album.Songs?.FirstOrDefault(x => x.SongId == selectedSongId)?.SongArtist();
            if (artistToPromote.Nullify() != null)
            {
                album.SetTagValue(MetaTagIdentifier.AlbumArtist, artistToPromote);
                foreach (var song in album.Songs!)
                {
                    album.RemoveSongTagValue(song.SongId, MetaTagIdentifier.Artist);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                }

                await SaveAlbum(album, cancellationToken); 
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

    public async Task<OperationResult<bool>> SetYearToCurrent(long[] albumIds, CancellationToken cancellationToken = default)
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
            var year = DateTime.Now.Year;
            foreach (var selectedAlbumId in albumIds)
            {
                var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                if (album.IsValid(_configuration.Configuration))
                {
                    album.SetTagValue(MetaTagIdentifier.OrigAlbumYear, year);
                    foreach (var song in album.Songs ?? [])
                    {
                        album.SetSongTagValue(song.SongId, MetaTagIdentifier.OrigAlbumYear, year);
                        await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                    }

                    await SaveAlbum(album, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Set Year to current year.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> ReplaceGivenTextFromSongTitles(long albumId, string textToRemove, string? textToReplaceWith, CancellationToken cancellationToken = default)
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
            var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, albumId, cancellationToken);
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
                await SaveAlbum(album, cancellationToken); 
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

    public async Task<OperationResult<bool>> RemoveAllSongArtists(long[] albumIds, CancellationToken cancellationToken = default)
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
                var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                foreach (var song in album.Songs!)
                {
                    album.RemoveSongTagValue(song.SongId, MetaTagIdentifier.AlbumArtist);
                    album.RemoveSongTagValue(song.SongId, MetaTagIdentifier.Artist);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                }
                await SaveAlbum(album, cancellationToken); 
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

    public async Task<OperationResult<bool>> ReplaceAllSongArtistSeparators(long[] albumIds, CancellationToken cancellationToken = default)
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
                var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
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
                await SaveAlbum(album, cancellationToken); 
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Replace all Song Artist separators.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> RenumberSongs(long[] albumIds, CancellationToken cancellationToken = default)
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
                var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
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
                await SaveAlbum(album, cancellationToken);
                result = true;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Renumbering Songs.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> DeleteAllImagesForAlbums(long[] albumIds, CancellationToken cancellationToken = default)
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
                var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                var albumStagingDirInfo = new DirectoryInfo(Path.Combine(_directoryStaging, album.ToDirectoryName()));
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
                await SaveAlbum(album, cancellationToken);                 
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

    public async Task<OperationResult<bool>> RemoveArtistFromSongArtists(long[] albumIds, CancellationToken cancellationToken = default)
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
                var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                foreach (var song in album.Songs!)
                {
                    album.SetSongTagValue(song.SongId, MetaTagIdentifier.AlbumArtist, null);
                    await _editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                }
                await SaveAlbum(album, cancellationToken); 
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
    
    public async Task<OperationResult<bool>> SetAlbumsStatusToReviewed(long[] albumIds, CancellationToken cancellationToken = default)
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
                var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                album.Status = AlbumStatus.Reviewed;
                await SaveAlbum(album, cancellationToken);                 
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

    public async Task<OperationResult<bool>> DeleteAlbumsInStagingAsync(long[] albumIds, CancellationToken cancellationToken = default)
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
                var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                var albumStagingDirInfo = new DirectoryInfo(Path.Combine(_directoryStaging, album.ToDirectoryName()));
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

    public async Task<OperationResult<bool>> MoveAlbumsToLibraryAsync(long[] albumIds, CancellationToken cancellationToken = default)
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
                var album = await albumDiscoveryService.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                var albumStagingDirInfo = new DirectoryInfo(Path.Combine(_directoryStaging, album.ToDirectoryName()));
                var albumLibraryDirInfo = new DirectoryInfo(Path.Combine(_directoryLibrary, album.ToDirectoryName()));
                var doMove = SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.ProcessingMoveMelodeeJsonDataFileToLibrary]);
                MoveDirectory(albumStagingDirInfo.FullName, albumLibraryDirInfo.FullName, doMove ? null : Album.JsonFileName);
            }
            DirectoryStagingFileSystemDirectoryInfo.DeleteAllEmptyDirectories();
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
    /// This exists because in some systems where data is on one mapped drive it cannot be "Moved" to another mapped drive ("Cross link" error), it must be copied and then deleted.
    /// </summary>
    private static void MoveDirectory(string toMove, string destination, string? dontMoveFileName)
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
