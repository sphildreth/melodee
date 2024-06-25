using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using DirectoryInfo = Melodee.Common.Models.DirectoryInfo;

namespace Melodee.Plugins.Processor;

public interface IProcessorPlugin : IPlugin
{
    Task<OperationResult<bool>> ProcessDirectoryAsync(DirectoryInfo directoryInfo, CancellationToken cancellationToken = default);
}