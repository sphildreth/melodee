using Melodee.Common.Models;
using Melodee.Plugins.Validation.Models;

namespace Melodee.Plugins.Processor;

public interface IAlbumEditProcessor
{
    Task<OperationResult<ValidationResult>> DoMagic(long albumId, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> PromoteSongArtist(long albumId, long selectedSongId, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> SetYearToCurrent(long[] albumIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> ReplaceGivenTextFromSongTitles(long albumId, string textToRemove, string? textToReplaceWith, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> RemoveAllSongArtists(long[] albumIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> ReplaceAllSongArtistSeparators(long[] albumIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> RenumberSongs(long[] albumIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> DeleteAllImagesForAlbums(long[] albumIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> RemoveArtistFromSongArtists(long[] albumIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> RemoveFeaturingArtistsFromSongsArtist(long albumId, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> SetAlbumsStatusToReviewed(long[] albumIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> DeleteAlbumsInStagingAsync(long[] albumIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> MoveAlbumsToLibraryAsync(long[] albumIds, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> RemoveUnwantedTextFromAlbumTitle(long albumId, CancellationToken cancellationToken = default);
    
    Task<OperationResult<bool>> RemoveFeaturingArtistsFromSongTitle(long albumId, CancellationToken cancellationToken = default);
}
