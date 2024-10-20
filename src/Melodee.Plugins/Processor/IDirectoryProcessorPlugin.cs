using Melodee.Common.Models;
using Melodee.Plugins.Processor.Models;

namespace Melodee.Plugins.Processor;

public interface IDirectoryProcessorPlugin : IPlugin
{
    event EventHandler<FileSystemDirectoryInfo>? OnDirectoryProcessed;

    event EventHandler<string>? OnProcessingEvent;

    event EventHandler<int>? OnProcessingStart;

    void StopProcessing();

    /// <summary>
    /// Process given directory and return found Albums.
    /// </summary>
    /// <param name="fileSystemDirectoryInfo">Directory to get all Albums from</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Tuple of Albums collection and Total Number of Songs</returns>
    Task<OperationResult<(IEnumerable<Album>, int)>> AllAlbumsForDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default);

    Task<OperationResult<DirectoryProcessorResult>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default);
}
