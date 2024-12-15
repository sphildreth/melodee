using Melodee.Common.Models;

namespace Melodee.Plugins.MetaData.Directory.Nfo.Handlers;

public interface INfoHandler
{
    Task<bool> IsHandlerForNfoAsync(FileInfo fileInfo, CancellationToken cancellationToken = default);
    Task<bool> HandleNfoAsync(FileInfo fileInfo, CancellationToken cancellationToken = default);
}
