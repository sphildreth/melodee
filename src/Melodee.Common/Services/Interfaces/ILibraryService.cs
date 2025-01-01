using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Album = Melodee.Common.Models.Album;

namespace Melodee.Common.Services.Interfaces;

public interface ILibraryService
{
    Task<OperationResult<Library>> GetInboundLibraryAsync(CancellationToken cancellationToken = default);

    Task<OperationResult<Library>> GetUserImagesLibraryAsync(CancellationToken cancellationToken = default);

    Task<OperationResult<Library?>> GetByApiKeyAsync(Guid apiKey, CancellationToken cancellationToken = default);

    Task<OperationResult<Library?>> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<OperationResult<Library>> GetLibraryAsync(CancellationToken cancellationToken = default);

    Task<OperationResult<Library?>> PurgeLibraryAsync(int libraryId, CancellationToken cancellationToken = default);

    Task<OperationResult<Library>> GetStagingLibraryAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<Library>> ListAsync(PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> MoveAlbumsToLibrary(Library library, Album[] albums, CancellationToken cancellationToken = default);

    Task<OperationResult<LibraryScanHistory?>> CreateLibraryScanHistory(Library library, LibraryScanHistory libraryScanHistory, CancellationToken cancellationToken = default);
}
