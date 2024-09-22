using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Utility;
using Serilog;

namespace Melodee.Plugins.Scripting;

public sealed class PostDiscoveryScript(Configuration configuration) : IScriptPlugin
{
    private readonly Configuration _configuration = configuration;

    public string Id => "1F97BA5C-E5C3-4DA1-969D-DCFCB9720E07";

    public string DisplayName => nameof(PostDiscoveryScript);

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;

    public bool StopProcessing { get; } = false;

    public async Task<OperationResult<bool>> ProcessAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var result = false;
        string? scriptOutput = null;
        if (!string.IsNullOrWhiteSpace(_configuration.Scripting.PostDiscoveryScript))
        {
            var scriptFileInfo = new FileInfo(_configuration.Scripting.PostDiscoveryScript!);
            if (!scriptFileInfo.Exists)
            {
                return new OperationResult<bool>
                {
                    Errors = new[]
                    {
                        new Exception($"Unable to locate PostDiscoveryScript [{_configuration.Scripting.PostDiscoveryScript}]")
                    },
                    Data = false
                };
            }

            try
            {
                var argument = $" -d \"{fileSystemDirectoryInfo.Path}\" -r {(_configuration.PluginProcessOptions.DoDeleteOriginal ? "0" : "1")}";
                await $"bash {_configuration.Scripting.PostDiscoveryScript}{argument}".Bash();
                result = true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error running PostDiscoveryScript");
                result = false;
            }
        }

        return new OperationResult<bool>
        {
            AdditionalData = new Dictionary<string, object>
            {
                { "output", scriptOutput ?? string.Empty }
            },
            Data = result
        };
    }
}
