using Melodee.Common.Models;
using Melodee.Plugins.Validation.Models;

namespace Melodee.Plugins.Processor;

public interface IReleaseEditProcessor
{
    Task<OperationResult<ValidationResult>> DoMagic(long releaseId, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> PromoteTrackArtist(long releaseId, long selectedTrackId, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> SetYearToCurrent(long[] releaseIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> ReplaceGivenTextFromTrackTitles(long releaseId, string textToRemove, string? textToReplaceWith, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> RemoveAllTrackArtists(long[] releaseIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> ReplaceAllTrackArtistSeparators(long[] releaseIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> RenumberTracks(long[] releaseIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> DeleteAllImagesForReleases(long[] releaseIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> RemoveArtistFromTrackArtists(long[] releaseIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> RemoveFeaturingArtistsFromTracksArtist(long releaseId, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> SetReleasesStatusToReviewed(long[] releaseIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> DeleteReleasesInStagingAsync(long[] releaseIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> MoveReleasesToLibraryAsync(long[] releaseIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> RemoveUnwantedTextFromReleaseTitle(long releaseId, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> RemoveFeaturingArtistsFromTrackTitle(long releaseId, CancellationToken cancellationToken = default);
}
