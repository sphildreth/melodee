using Melodee.Common.Models;
using DirectoryInfo = Melodee.Common.Models.DirectoryInfo;

namespace Melodee.Plugins.Discovery.Releases;

/// <summary>
/// For the given Directory find all Releases, gathered by enabled plugins for various metadata sources.
/// </summary>
public interface IReleasesDiscoverer : IPlugin
{
    /// <summary>
    /// Return a paged collection of Releases in given Directory.
    /// </summary>
    /// <param name="directoryInfo">Directory to load models from.</param>
    /// <param name="pagedRequest">Pagination Request.</param> 
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Paged Result of Release models found in directory</returns>
    Task<PagedResult<Release>> ReleasesForDirectoryAsync(
        DirectoryInfo directoryInfo,
        PagedRequest pagedRequest, 
        CancellationToken cancellationToken = default);    
    
    Task<Release> ReleaseByUniqueIdAsync(
        DirectoryInfo directoryInfo,
        long uniqueId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Return a paged collection of ReleaseGrid models used to populate the main grid.
    /// </summary>
    /// <param name="directoryInfo">Directory to load models from.</param>
    /// <param name="pagedRequest">Pagination Request.</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Paged Result of ReleaseGrid models found in directory</returns>
    Task<PagedResult<Common.Models.Grids.ReleaseGrid>> ReleasesGridsForDirectoryAsync(
        DirectoryInfo directoryInfo, 
        PagedRequest pagedRequest, 
        CancellationToken cancellationToken = default);
}