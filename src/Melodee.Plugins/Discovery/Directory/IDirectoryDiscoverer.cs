using Melodee.Common.Models;

namespace Melodee.Plugins.Discovery.Directory;

public interface IDirectoryDiscoverer : IPlugin
{
    /// <summary>
    /// Process given Directory and return any DirectoryInfos.
    /// </summary>
    /// <param name="directoryInfo">DirectoryInfo to process.</param>
    /// <param name="pagedRequest">Paged Request info; take, skip, filter, etc.</param>
    /// <returns>PagedResult of Discovered DirectoryInfos.</returns>
    OperationResult<PagedResult<Common.Models.DirectoryInfo>> DirectoryInfosForDirectory(System.IO.DirectoryInfo directoryInfo, PagedRequest pagedRequest);
}