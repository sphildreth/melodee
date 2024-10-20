using System.Text.Json;
using System.Text.Json.Serialization;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.Discovery.Releases;
using Melodee.Plugins.MetaData.Track;
using Melodee.Plugins.Validation;
using Melodee.Plugins.Validation.Models;
using Serilog;

namespace Melodee.Plugins.Processor;

public sealed class ReleaseEditProcessor(
    Configuration configuration,
    IReleasesDiscoverer releasesDiscoverer,
    ITrackPlugin editTrackPlugin,
    IReleaseValidator releaseValidator)
    : IReleaseEditProcessor
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<OperationResult<ValidationResult>> DoMagic(long releaseId, CancellationToken cancellationToken = default)
    {
        if (!configuration.MagicOptions.IsMagicEnabled)
        {
            return new OperationResult<ValidationResult>
            {
                Data = new ValidationResult
                {
                    ReleaseStatus = ReleaseStatus.NotSet
                }
            };
        }
        if (configuration.MagicOptions.DoRenumberTracks)
        {
            await RenumberTracks([releaseId], cancellationToken);
        }
        if (configuration.MagicOptions.DoRemoveFeaturingArtistFromTracksArtist)
        {
            await RemoveFeaturingArtistsFromTracksArtist(releaseId, cancellationToken);
        }
        if (configuration.MagicOptions.DoRemoveFeaturingArtistFromTrackTitle)
        {
            await RemoveFeaturingArtistsFromTrackTitle(releaseId, cancellationToken);
        }
        if (configuration.MagicOptions.DoReplaceTracksArtistSeperators)
        {
            await ReplaceAllTrackArtistSeparators([releaseId], cancellationToken);
        }
        if (configuration.MagicOptions.DoSetYearToCurrentIfInvalid)
        {
            await SetYearToCurrent([releaseId], cancellationToken);
        }       
        if (configuration.MagicOptions.DoRemoveUnwantedTextFromReleaseTitle)
        {
            await RemoveUnwantedTextFromReleaseTitle(releaseId, cancellationToken);
        }         
        var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, releaseId, cancellationToken);
        var validationResult = releaseValidator.ValidateRelease(release);
        release.Status = validationResult.Data.ReleaseStatus;
        release.Modified = DateTimeOffset.UtcNow;
        await SaveRelease(release, cancellationToken);
        return validationResult;
    }

    private async Task SaveRelease(Release release, CancellationToken cancellationToken = default)
    {
        var serialized = JsonSerializer.Serialize(release, _jsonSerializerOptions);
        var releaseDirectoryName = release.ToDirectoryName();
        if (releaseDirectoryName.Nullify() != null)
        {
            var releaseStagingDirInfo = new DirectoryInfo(Path.Combine(configuration.StagingDirectory, releaseDirectoryName));
            var jsonName = Path.Combine(releaseStagingDirInfo.FullName, release.ToMelodeeJsonName(true));
            try
            {
                await File.WriteAllTextAsync(jsonName, serialized, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[{Release}] JsonName [{JsonName}]", release.ToString(), jsonName);
            }
        }
        else
        {
            Log.Warning("[{Release}] has invalid Directory Name [{ReleaseDirectoryName}]", release.ToString(), releaseDirectoryName);
        }
    }

    public async Task<OperationResult<bool>> RemoveUnwantedTextFromReleaseTitle(long releaseId, CancellationToken cancellationToken = default)
    {
        if (releaseId < 1)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }
        var result = false;
        try
        {
            var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, releaseId, cancellationToken);
            if (release.Tracks?.Count() > 0)
            {
                var title = release.ReleaseTitle();
                var newTitle = ReleaseValidator.RemoveUnwantedTextFromReleaseTitle(title);
                if (!string.Equals(title, newTitle, StringComparison.OrdinalIgnoreCase))
                {
                    release.SetTagValue(MetaTagIdentifier.Album, newTitle);
                    foreach (var track in release.Tracks)
                    {
                        release.SetTrackTagValue(track.TrackId, MetaTagIdentifier.Album, newTitle);
                        await editTrackPlugin.UpdateTrackAsync(release.Directory!, track, cancellationToken);
                    }
                    await SaveRelease(release, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Remove featuring Artists from Tracks Artist.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };        
    }
    
    public async Task<OperationResult<bool>> RemoveFeaturingArtistsFromTrackTitle(long releaseId, CancellationToken cancellationToken = default)
    {
        if (releaseId < 1)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, releaseId, cancellationToken);
            if (release.Tracks?.Count() > 0)
            {
                foreach (var track in release.Tracks)
                {
                    var trackTitle = track.Title();
                    if (trackTitle != null && ReleaseValidator.StringHasFeaturingFragments(trackTitle))
                    {
                        var matches = ReleaseValidator.HasFeatureFragmentsRegex.Match(trackTitle);
                        string? newTrackTitle = ReleaseValidator.ReplaceTrackArtistSeparators(ReleaseValidator.HasFeatureFragmentsRegex.Replace(trackTitle.Substring(matches.Index), string.Empty).CleanString());
                        newTrackTitle = newTrackTitle?.TrimEnd(']', ')').Replace("\"", "'");
                        release.SetTrackTagValue(track.TrackId, MetaTagIdentifier.Title, newTrackTitle);
                        await editTrackPlugin.UpdateTrackAsync(release.Directory!, track, cancellationToken);
                    }
                }
                await SaveRelease(release, cancellationToken);                
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Remove featuring Artists from Tracks Artist.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }    

    public async Task<OperationResult<bool>> RemoveFeaturingArtistsFromTracksArtist(long releaseId, CancellationToken cancellationToken = default)
    {
        if (releaseId < 1)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, releaseId, cancellationToken);
            foreach (var track in release.Tracks?.Where(t => t.TrackArtist().Nullify() != null) ?? [])
            {
                var trackArtist = track.TrackArtist();
                if (trackArtist != null && ReleaseValidator.StringHasFeaturingFragments(trackArtist))
                {
                    var matches = ReleaseValidator.HasFeatureFragmentsRegex.Match(trackArtist);
                    string? newTrackArtist = ReleaseValidator.ReplaceTrackArtistSeparators(ReleaseValidator.HasFeatureFragmentsRegex.Replace(trackArtist.Substring(matches.Index), string.Empty).CleanString());
                    newTrackArtist = newTrackArtist?.TrimEnd(']', ')').Replace("\"", "'");
                    release.SetTrackTagValue(track.TrackId, MetaTagIdentifier.Artist, newTrackArtist);
                    await editTrackPlugin.UpdateTrackAsync(release.Directory!, track, cancellationToken);
                }
            }
            await SaveRelease(release, cancellationToken); 
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Remove featuring Artists from Tracks Artist.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> PromoteTrackArtist(long releaseId, long selectedTrackId, CancellationToken cancellationToken = default)
    {
        if (releaseId < 1)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, releaseId, cancellationToken);
            var artistToPromote = release.Tracks?.FirstOrDefault(x => x.TrackId == selectedTrackId)?.TrackArtist();
            if (artistToPromote.Nullify() != null)
            {
                release.SetTagValue(MetaTagIdentifier.AlbumArtist, artistToPromote);
                foreach (var track in release.Tracks!)
                {
                    release.RemoveTrackTagValue(track.TrackId, MetaTagIdentifier.Artist);
                    await editTrackPlugin.UpdateTrackAsync(release.Directory!, track, cancellationToken);
                }

                await SaveRelease(release, cancellationToken); 
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Promote Track Artist to Release Artist.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> SetYearToCurrent(long[] releaseIds, CancellationToken cancellationToken = default)
    {
        if (releaseIds.Length == 0)
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
            foreach (var selectedReleaseId in releaseIds)
            {
                var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, selectedReleaseId, cancellationToken);
                if (release?.IsValid(configuration) ?? false)
                {
                    release.SetTagValue(MetaTagIdentifier.OrigReleaseYear, year);
                    foreach (var track in release.Tracks ?? [])
                    {
                        release.SetTrackTagValue(track.TrackId, MetaTagIdentifier.OrigReleaseYear, year);
                        await editTrackPlugin.UpdateTrackAsync(release.Directory!, track, cancellationToken);
                    }

                    await SaveRelease(release, cancellationToken);
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

    public async Task<OperationResult<bool>> ReplaceGivenTextFromTrackTitles(long releaseId, string textToRemove, string? textToReplaceWith, CancellationToken cancellationToken = default)
    {
        if (releaseId < 1 || textToRemove.Nullify() == null)
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
            var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, releaseId, cancellationToken);
            foreach (var track in release.Tracks!)
            {
                var originalTitle = track.Title() ?? string.Empty;
                var newTitle = track.Title()!.Replace(originalTitle, textToReplaceWith ?? string.Empty).Trim();
                if (!string.Equals(originalTitle, newTitle, StringComparison.OrdinalIgnoreCase))
                {
                    release.SetTrackTagValue(track.TrackId, MetaTagIdentifier.Title, newTitle);
                    await editTrackPlugin.UpdateTrackAsync(release.Directory!, track, cancellationToken);
                    modified = true;
                }
            }

            if (modified)
            {
                await SaveRelease(release, cancellationToken); 
                releasesDiscoverer.ClearCache();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Replace text with given text for all Track Titles.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> RemoveAllTrackArtists(long[] releaseIds, CancellationToken cancellationToken = default)
    {
        if (releaseIds.Length == 0)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            foreach (var selectedReleaseId in releaseIds)
            {
                var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, selectedReleaseId, cancellationToken);
                foreach (var track in release.Tracks!)
                {
                    release.RemoveTrackTagValue(track.TrackId, MetaTagIdentifier.AlbumArtist);
                    release.RemoveTrackTagValue(track.TrackId, MetaTagIdentifier.Artist);
                    await editTrackPlugin.UpdateTrackAsync(release.Directory!, track, cancellationToken);
                }
                await SaveRelease(release, cancellationToken); 
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Remove all Track Artists.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> ReplaceAllTrackArtistSeparators(long[] releaseIds, CancellationToken cancellationToken = default)
    {
        if (releaseIds.Length == 0)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            foreach (var selectedReleaseId in releaseIds)
            {
                var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, selectedReleaseId, cancellationToken);
                foreach (var track in release.Tracks?.Where(x => x.TrackArtist().Nullify() != null) ?? [])
                {
                    var oldTrackArtist = track.TrackArtist();
                    var newTrackArtist = MetaTagsProcessor.ReplaceTrackArtistSeparators(oldTrackArtist);
                    if (!string.Equals(oldTrackArtist, newTrackArtist, StringComparison.OrdinalIgnoreCase))
                    {
                        release.SetTrackTagValue(track.TrackId, MetaTagIdentifier.AlbumArtist, null);
                        await editTrackPlugin.UpdateTrackAsync(release.Directory!, track, cancellationToken);
                    }
                }
                await SaveRelease(release, cancellationToken); 
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Replace all Track Artist separators.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> RenumberTracks(long[] releaseIds, CancellationToken cancellationToken = default)
    {
        if (releaseIds.Length == 0)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            foreach (var selectedReleaseId in releaseIds)
            {
                var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, selectedReleaseId, cancellationToken);
                var numberOfMedias = release.MediaCountValue();
                var mediaLooper = 0;
                while (mediaLooper <= numberOfMedias)
                {
                    var looper = mediaLooper;
                    foreach (var dd in release.Tracks?.Where(x => x.MediaNumber() == looper).Select((x, i) => new { x, i = i + 1 }) ?? [])
                    {
                        release.SetTrackTagValue(dd.x.TrackId, MetaTagIdentifier.TrackNumber, dd.i);
                        await editTrackPlugin.UpdateTrackAsync(release.Directory!, dd.x, cancellationToken);
                    }

                    mediaLooper++;
                }
                await SaveRelease(release, cancellationToken);
                result = true;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Renumbering Tracking.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }


    public async Task<OperationResult<bool>> DeleteAllImagesForReleases(long[] releaseIds, CancellationToken cancellationToken = default)
    {
        if (releaseIds.Length == 0)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            foreach (var selectedReleaseId in releaseIds)
            {
                var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, selectedReleaseId, cancellationToken);
                var releaseStagingDirInfo = new DirectoryInfo(Path.Combine(configuration.StagingDirectory, release.ToDirectoryName()));
                release.Images = [];
                var serialized = JsonSerializer.Serialize(release, _jsonSerializerOptions);
                await File.WriteAllTextAsync(Path.Combine(releaseStagingDirInfo.FullName, release.ToMelodeeJsonName(true)), serialized, cancellationToken);
                foreach (var imageFile in ImageHelper.ImageFilesInDirectory(releaseStagingDirInfo.FullName, SearchOption.AllDirectories))
                {
                    File.Delete(imageFile);
                }

                foreach (var track in release.Tracks!)
                {
                    track.Images = [];
                    await editTrackPlugin.UpdateTrackAsync(release.Directory!, track, cancellationToken);
                }
                await SaveRelease(release, cancellationToken);                 
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Deleting all images for releases.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> RemoveArtistFromTrackArtists(long[] releaseIds, CancellationToken cancellationToken = default)
    {
        if (releaseIds.Length == 0)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            foreach (var selectedReleaseId in releaseIds)
            {
                var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, selectedReleaseId, cancellationToken);
                foreach (var track in release.Tracks!)
                {
                    release.SetTrackTagValue(track.TrackId, MetaTagIdentifier.AlbumArtist, null);
                    await editTrackPlugin.UpdateTrackAsync(release.Directory!, track, cancellationToken);
                }
                await SaveRelease(release, cancellationToken); 
            }

            result = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Remove Artist from Track Artists.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }


    public async Task<OperationResult<bool>> SetReleasesStatusToReviewed(long[] releaseIds, CancellationToken cancellationToken = default)
    {
        if (releaseIds.Length == 0)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            foreach (var selectedReleaseId in releaseIds)
            {
                var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, selectedReleaseId, cancellationToken);
                release.Status = ReleaseStatus.Reviewed;
                await SaveRelease(release, cancellationToken);                 
            }

            result = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Set Releases status to reviewed.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> DeleteReleasesInStagingAsync(long[] releaseIds, CancellationToken cancellationToken = default)
    {
        if (releaseIds.Length == 0)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            foreach (var selectedReleaseId in releaseIds)
            {
                var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, selectedReleaseId, cancellationToken);
                var releaseStagingDirInfo = new DirectoryInfo(Path.Combine(configuration.StagingDirectory, release.ToDirectoryName()));
                try
                {
                    Directory.Delete(releaseStagingDirInfo.FullName, true);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error deleting [{ReleaseId}]", selectedReleaseId);
                }
            }

            result = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Deleting Releases from staging.");
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> MoveReleasesToLibraryAsync(long[] releaseIds, CancellationToken cancellationToken = default)
    {
        if (releaseIds.Length == 0)
        {
            return new OperationResult<bool>
            {
                Data = false
            };
        }

        var result = false;
        try
        {
            foreach (var selectedReleaseId in releaseIds)
            {
                var release = await releasesDiscoverer.ReleaseByUniqueIdAsync(configuration.StagingDirectoryInfo, selectedReleaseId, cancellationToken);
                var releaseStagingDirInfo = new DirectoryInfo(Path.Combine(configuration.StagingDirectory, release.ToDirectoryName()));
                var releaseLibraryDirInfo = new DirectoryInfo(Path.Combine(configuration.LibraryDirectory, release.ToDirectoryName()));
                MoveDirectory(releaseStagingDirInfo.FullName, releaseLibraryDirInfo.FullName, configuration.MoveMelodeeJsonDataFileToLibrary ? null : Release.JsonFileName);
            }

            releasesDiscoverer.ClearCache();
            configuration.StagingDirectoryInfo.DeleteAllEmptyDirectories();
            result = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Moving Releases To library.");
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
