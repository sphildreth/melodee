using Melodee.Common.Models;
using Melodee.Common.Models.Cards;

namespace Melodee.Plugins.Discovery.Albums;

/// <summary>
///     For the given Directory find all Albums, gathered by enabled plugins for various metadata sources.
/// </summary>
public interface IAlbumsDiscoverer : IPlugin
{
    /// <summary>
    ///     Return a paged collection of Albums in given Directory.
    /// </summary>
    /// <param name="fileSystemDirectoryInfo">Directory to load models from.</param>
    /// <param name="pagedRequest">Pagination Request.</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Paged Result of Album models found in directory</returns>
    Task<PagedResult<Album>> AlbumsForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default);

    Task<Album> AlbumByUniqueIdAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        long uniqueId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Return a paged collection of AlbumGrid models used to populate the main grid.
    /// </summary>
    /// <param name="fileSystemDirectoryInfo">Directory to load models from.</param>
    /// <param name="pagedRequest">Pagination Request.</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Paged Result of AlbumGrid models found in directory</returns>
    Task<PagedResult<AlbumCard>> AlbumsGridsForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Clear previously found Albums cache.
    /// </summary>
    void ClearCache();
}
