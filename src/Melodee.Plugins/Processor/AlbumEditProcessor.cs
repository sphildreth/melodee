using System.Text.Json;
using System.Text.Json.Serialization;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;

using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.Discovery.Albums;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Validation;
using Melodee.Plugins.Validation.Models;
using Serilog;

namespace Melodee.Plugins.Processor;

public sealed class AlbumEditProcessor(
    Dictionary<string, object?> configuration,
    IAlbumsDiscoverer albumsDiscoverer,
    ISongPlugin editSongPlugin,
    IAlbumValidator albumValidator)
    : IAlbumEditProcessor
{

    private string DirectoryStaging => SafeParser.ToString(configuration[SettingRegistry.DirectoryStaging]);
    private DirectoryInfo DirectoryStagingInfo => new DirectoryInfo(DirectoryStaging);
    private FileSystemDirectoryInfo DirectoryStagingFileSystemDirectoryInfo => DirectoryStagingInfo.ToDirectorySystemInfo();
    private string DirectoryLibrary => SafeParser.ToString(configuration[SettingRegistry.DirectoryLibrary]);
    
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<OperationResult<ValidationResult>> DoMagic(long albumId, CancellationToken cancellationToken = default)
    {
        if (!SafeParser.ToBoolean(configuration[SettingRegistry.MagicEnabled]))
        {
            return new OperationResult<ValidationResult>
            {
                Data = new ValidationResult
                {
                    AlbumStatus = AlbumStatus.NotSet
                }
            };
        }
        if ( SafeParser.ToBoolean(configuration[SettingRegistry.MagicDoRenumberSongs]))
        {
            await RenumberSongs([albumId], cancellationToken);
        }
        if (SafeParser.ToBoolean(configuration[SettingRegistry.MagicDoRemoveFeaturingArtistFromSongArtist]))
        {
            await RemoveFeaturingArtistsFromSongsArtist(albumId, cancellationToken);
        }
        if (SafeParser.ToBoolean(configuration[SettingRegistry.MagicDoRemoveFeaturingArtistFromSongTitle]))
        {
            await RemoveFeaturingArtistsFromSongTitle(albumId, cancellationToken);
        }
        if (SafeParser.ToBoolean(configuration[SettingRegistry.MagicDoReplaceSongsArtistSeparators]))
        {
            await ReplaceAllSongArtistSeparators([albumId], cancellationToken);
        }
        if (SafeParser.ToBoolean(configuration[SettingRegistry.MagicDoSetYearToCurrentIfInvalid]))
        {
            await SetYearToCurrent([albumId], cancellationToken);
        }       
        if (SafeParser.ToBoolean(configuration[SettingRegistry.MagicDoRemoveUnwantedTextFromAlbumTitle]))
        {
            await RemoveUnwantedTextFromAlbumTitle(albumId, cancellationToken);
        }
        var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, albumId, cancellationToken);
        var validationResult = albumValidator.ValidateAlbum(album);
        album.Status = validationResult.Data.AlbumStatus;
        album.Modified = DateTimeOffset.UtcNow;
        await SaveAlbum(album, cancellationToken);
        return validationResult;
    }

    private async Task SaveAlbum(Album album, CancellationToken cancellationToken = default)
    {
        var serialized = JsonSerializer.Serialize(album, _jsonSerializerOptions);
        var albumDirectoryName = album.ToDirectoryName();
        if (albumDirectoryName.Nullify() != null)
        {
            var albumStagingDirInfo = new DirectoryInfo(Path.Combine(DirectoryStaging, albumDirectoryName));
            var jsonName = Path.Combine(albumStagingDirInfo.FullName, album.ToMelodeeJsonName(true));
            try
            {
                await File.WriteAllTextAsync(jsonName, serialized, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[{Album}] JsonName [{JsonName}]", album.ToString(), jsonName);
            }
        }
        else
        {
            Log.Warning("[{Album}] has invalid Directory Name [{AlbumDirectoryName}]", album.ToString(), albumDirectoryName);
        }
    }

    public async Task<OperationResult<bool>> RemoveUnwantedTextFromAlbumTitle(long albumId, CancellationToken cancellationToken = default)
    {
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
            var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, albumId, cancellationToken);
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
                        await editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
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
            var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, albumId, cancellationToken);
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
                        await editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
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
            var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, albumId, cancellationToken);
            foreach (var song in album.Songs?.Where(t => t.SongArtist().Nullify() != null) ?? [])
            {
                var songArtist = song.SongArtist();
                if (songArtist != null && AlbumValidator.StringHasFeaturingFragments(songArtist))
                {
                    var matches = AlbumValidator.HasFeatureFragmentsRegex.Match(songArtist);
                    string? newSongArtist = AlbumValidator.ReplaceSongArtistSeparators(AlbumValidator.HasFeatureFragmentsRegex.Replace(songArtist.Substring(matches.Index), string.Empty).CleanString());
                    newSongArtist = newSongArtist?.TrimEnd(']', ')').Replace("\"", "'");
                    album.SetSongTagValue(song.SongId, MetaTagIdentifier.Artist, newSongArtist);
                    await editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
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
            var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, albumId, cancellationToken);
            var artistToPromote = album.Songs?.FirstOrDefault(x => x.SongId == selectedSongId)?.SongArtist();
            if (artistToPromote.Nullify() != null)
            {
                album.SetTagValue(MetaTagIdentifier.AlbumArtist, artistToPromote);
                foreach (var song in album.Songs!)
                {
                    album.RemoveSongTagValue(song.SongId, MetaTagIdentifier.Artist);
                    await editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
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
                var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                if (album.IsValid(configuration))
                {
                    album.SetTagValue(MetaTagIdentifier.OrigAlbumYear, year);
                    foreach (var song in album.Songs ?? [])
                    {
                        album.SetSongTagValue(song.SongId, MetaTagIdentifier.OrigAlbumYear, year);
                        await editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
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
            var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, albumId, cancellationToken);
            foreach (var song in album.Songs!)
            {
                var originalTitle = song.Title() ?? string.Empty;
                var newTitle = song.Title()!.Replace(originalTitle, textToReplaceWith ?? string.Empty).Trim();
                if (!string.Equals(originalTitle, newTitle, StringComparison.OrdinalIgnoreCase))
                {
                    album.SetSongTagValue(song.SongId, MetaTagIdentifier.Title, newTitle);
                    await editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
                    modified = true;
                }
            }

            if (modified)
            {
                await SaveAlbum(album, cancellationToken); 
                albumsDiscoverer.ClearCache();
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
                var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                foreach (var song in album.Songs!)
                {
                    album.RemoveSongTagValue(song.SongId, MetaTagIdentifier.AlbumArtist);
                    album.RemoveSongTagValue(song.SongId, MetaTagIdentifier.Artist);
                    await editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
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
                var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                foreach (var song in album.Songs?.Where(x => x.SongArtist().Nullify() != null) ?? [])
                {
                    var oldSongArtist = song.SongArtist();
                    var newSongArtist = MetaTagsProcessor.ReplaceSongArtistSeparators(oldSongArtist);
                    if (!string.Equals(oldSongArtist, newSongArtist, StringComparison.OrdinalIgnoreCase))
                    {
                        album.SetSongTagValue(song.SongId, MetaTagIdentifier.AlbumArtist, null);
                        await editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
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
                var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                var numberOfMedias = album.MediaCountValue();
                var mediaLooper = 0;
                while (mediaLooper <= numberOfMedias)
                {
                    var looper = mediaLooper;
                    foreach (var dd in album.Songs?.Where(x => x.MediaNumber() == looper).Select((x, i) => new { x, i = i + 1 }) ?? [])
                    {
                        album.SetSongTagValue(dd.x.SongId, MetaTagIdentifier.TrackNumber, dd.i);
                        await editSongPlugin.UpdateSongAsync(album.Directory!, dd.x, cancellationToken);
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
                var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                var albumStagingDirInfo = new DirectoryInfo(Path.Combine(DirectoryStaging, album.ToDirectoryName()));
                album.Images = [];
                var serialized = JsonSerializer.Serialize(album, _jsonSerializerOptions);
                await File.WriteAllTextAsync(Path.Combine(albumStagingDirInfo.FullName, album.ToMelodeeJsonName(true)), serialized, cancellationToken);
                foreach (var imageFile in ImageHelper.ImageFilesInDirectory(albumStagingDirInfo.FullName, SearchOption.AllDirectories))
                {
                    File.Delete(imageFile);
                }

                foreach (var song in album.Songs!)
                {
                    song.Images = [];
                    await editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
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
                var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                foreach (var song in album.Songs!)
                {
                    album.SetSongTagValue(song.SongId, MetaTagIdentifier.AlbumArtist, null);
                    await editSongPlugin.UpdateSongAsync(album.Directory!, song, cancellationToken);
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
                var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
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
                var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                var albumStagingDirInfo = new DirectoryInfo(Path.Combine(DirectoryStaging, album.ToDirectoryName()));
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
                var album = await albumsDiscoverer.AlbumByUniqueIdAsync(DirectoryStagingFileSystemDirectoryInfo, selectedAlbumId, cancellationToken);
                var albumStagingDirInfo = new DirectoryInfo(Path.Combine(DirectoryStaging, album.ToDirectoryName()));
                var albumLibraryDirInfo = new DirectoryInfo(Path.Combine(DirectoryLibrary, album.ToDirectoryName()));
                var doMove = SafeParser.ToBoolean(configuration[SettingRegistry.ProcessingMoveMelodeeJsonDataFileToLibrary]);
                MoveDirectory(albumStagingDirInfo.FullName, albumLibraryDirInfo.FullName, doMove ? null : Album.JsonFileName);
            }

            albumsDiscoverer.ClearCache();
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
