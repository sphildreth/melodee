using Melodee.Common.Models;
using FileInfo = Melodee.Common.Models.FileInfo;

namespace Melodee.Plugins.Discovery.File;

/// <summary>
/// For the given Directory find all files to be processed by additional plugins.
/// </summary>
public interface IFileDiscoverer : IPlugin
{
    OperationResult<PagedResult<FileInfo>> FileInfosForDirectory(System.IO.DirectoryInfo directoryInfo, PagedRequest pagedRequest);
}