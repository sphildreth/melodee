using Melodee.Common.Models;

namespace Melodee.Plugins.Discovery.Directories;

/// <summary>
/// For the given Directory find all other non-empty Directories. 
/// </summary>
public interface IDirectoriesDiscoverer : IPlugin
{
    /// <summary>
    /// Process given Directory and return any DirectoryInfos.
    /// </summary>
    /// <param name="directoryInfo">DirectoryInfo to process.</param>
    /// <param name="pagedRequest">Paged Request info; take, skip, filter, etc.</param>
    /// <returns>PagedResult of Discovered DirectoryInfos.</returns>
    PagedResult<Common.Models.DirectoryInfo> DirectoryInfosForDirectory(System.IO.DirectoryInfo directoryInfo, PagedRequest pagedRequest);
}