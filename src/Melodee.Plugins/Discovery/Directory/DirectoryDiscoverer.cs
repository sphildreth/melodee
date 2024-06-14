using Melodee.Common.Models;

namespace Melodee.Plugins.Discovery.Directory;

public sealed class DirectoryDiscoverer : IDirectoryDiscoverer
{
    public DirectoryDiscoverer()
    {
    }

    public string DisplayName => nameof(DirectoryDiscoverer);

    public string Id => "EBAD92EA-A49E-4502-818D-D6B36B9E5993";

    public bool IsEnabled { get; set; } = true;
    
    public int SortOrder => 0;
    
    public Task<OperationResult<PagedResult<Common.Models.DirectoryInfo>>> DirectoryInfosForDirectory(System.IO.DirectoryInfo directoryInfo, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }


}