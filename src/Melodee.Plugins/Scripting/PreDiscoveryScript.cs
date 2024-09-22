using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Utility;
using Serilog;

namespace Melodee.Plugins.Scripting;

public sealed class PreDiscoveryScript(Configuration configuration) : IScriptPlugin
{
    private readonly Configuration _configuration = configuration;

    public string Id => "837CE3BD-F854-4B8D-A64A-978649AAB08A";

    public string DisplayName => nameof(PreDiscoveryScript);

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;

    public bool StopProcessing { get; } = false;

    public async Task<OperationResult<bool>> ProcessAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var result = false;
        string? scriptOutput = null;
        if (!string.IsNullOrWhiteSpace(_configuration.Scripting.PreDiscoveryScript))
        {
            var scriptFileInfo = new FileInfo(_configuration.Scripting.PreDiscoveryScript!);
            if (!scriptFileInfo.Exists)
            {
                return new OperationResult<bool>
                {
                    Errors = new[]
                    {
                        new Exception($"Unable to locate PreDiscoveryScript [{_configuration.Scripting.PreDiscoveryScript}]")
                    },
                    Data = false
                };
            }

            try
            {
                var argument = $" -d \"{fileSystemDirectoryInfo.Path}\" -r {(_configuration.PluginProcessOptions.DoDeleteOriginal ? "0" : "1")}";
                await $"bash {_configuration.Scripting.PreDiscoveryScript}{argument}".Bash();
                result = true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error running PreDiscoveryScript");
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
