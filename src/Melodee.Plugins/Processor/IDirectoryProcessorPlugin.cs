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
    /// Process given directory and return found Releases.
    /// </summary>
    /// <param name="fileSystemDirectoryInfo">Directory to get all Releases from</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Tuple of Releases collection and Total Number of Tracks</returns>
    Task<OperationResult<(IEnumerable<Release>, int)>> AllReleasesForDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default);

    Task<OperationResult<DirectoryProcessorResult>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default);
}
