using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;

namespace Melodee.Plugins.Scripting;

/// <summary>
///     Do nothing script used when no script is desired or configured.
/// </summary>
public sealed class NullScript(Configuration configuration) : IScriptPlugin
{
    private readonly Configuration _configuration = configuration;

    public string Id => "1C4F80FF-C226-4C5B-A53A-B12331534725";

    public string DisplayName => nameof(NullScript);

    public bool IsEnabled { get; set; } = false;

    public int SortOrder { get; } = 0;

    public bool StopProcessing { get; } = false;

    public Task<OperationResult<bool>> ProcessAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
