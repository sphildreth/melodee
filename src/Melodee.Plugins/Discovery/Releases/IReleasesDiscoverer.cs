using Melodee.Common.Models;
using Melodee.Common.Models.Cards;

namespace Melodee.Plugins.Discovery.Releases;

/// <summary>
/// For the given Directory find all Releases, gathered by enabled plugins for various metadata sources.
/// </summary>
public interface IReleasesDiscoverer : IPlugin
{
    /// <summary>
    /// Return a paged collection of Releases in given Directory.
    /// </summary>
    /// <param name="fileSystemDirectoryInfo">Directory to load models from.</param>
    /// <param name="pagedRequest">Pagination Request.</param> 
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Paged Result of Release models found in directory</returns>
    Task<PagedResult<Release>> ReleasesForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        PagedRequest pagedRequest, 
        CancellationToken cancellationToken = default);    
    
    Task<Release> ReleaseByUniqueIdAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        long uniqueId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Return a paged collection of ReleaseGrid models used to populate the main grid.
    /// </summary>
    /// <param name="fileSystemDirectoryInfo">Directory to load models from.</param>
    /// <param name="pagedRequest">Pagination Request.</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Paged Result of ReleaseGrid models found in directory</returns>
    Task<PagedResult<ReleaseCard>> ReleasesGridsForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo, 
        PagedRequest pagedRequest, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clear previously found Releases cache.
    /// </summary>
    void ClearCache();
}
