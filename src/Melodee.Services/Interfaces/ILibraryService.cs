using Melodee.Common.Data.Models;

namespace Melodee.Services.Interfaces;

public interface ILibraryService
{
    Task<Common.Models.OperationResult<Library>> GetInboundLibraryAsync(CancellationToken cancellationToken = default);
    
    Task<Common.Models.OperationResult<Library?>> GetByApiKeyAsync(Guid apiKey, CancellationToken cancellationToken = default);
    
    Task<Common.Models.OperationResult<Library?>> GetAsync(int id, CancellationToken cancellationToken = default);
    
    Task<Common.Models.OperationResult<Library>> GetLibraryAsync(CancellationToken cancellationToken = default);
    
    Task<Common.Models.OperationResult<Library?>> PurgeLibraryAsync(int libraryId, CancellationToken cancellationToken = default);
    
    Task<Common.Models.OperationResult<Library>> GetStagingLibraryAsync(CancellationToken cancellationToken = default);
    
    Task<Common.Models.PagedResult<Library>> ListAsync(Common.Models.PagedRequest pagedRequest, CancellationToken cancellationToken = default);
    
    Task<Common.Models.OperationResult<bool>> MoveAlbumsToLibrary(Library library, Common.Models.Album[] albums, CancellationToken cancellationToken = default);
    
    Task<Common.Models.OperationResult<LibraryScanHistory?>> CreateLibraryScanHistory(Library library, LibraryScanHistory libraryScanHistory, CancellationToken cancellationToken = default);
}
