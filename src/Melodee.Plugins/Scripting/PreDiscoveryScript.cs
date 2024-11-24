using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models;
using Melodee.Common.Utility;
using Serilog;

namespace Melodee.Plugins.Scripting;

public sealed class PreDiscoveryScript(IMelodeeConfiguration configuration) : IScriptPlugin
{
    private readonly Dictionary<string, object?> _configuration = configuration.Configuration;

    public string Id => "837CE3BD-F854-4B8D-A64A-978649AAB08A";

    public string DisplayName => nameof(PreDiscoveryScript);

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;

    public bool StopProcessing { get; } = false;

    public async Task<OperationResult<bool>> ProcessAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var result = false;
        string? scriptOutput = null;
        var script = SafeParser.ToString(_configuration[SettingRegistry.ScriptingPreDiscoveryScript]);
        if (!string.IsNullOrWhiteSpace(script))
        {
            var scriptFileInfo = new FileInfo(script!);
            if (!scriptFileInfo.Exists)
            {
                return new OperationResult<bool>
                {
                    Errors = new[]
                    {
                        new Exception($"Unable to locate PreDiscoveryScript [{script}]")
                    },
                    Data = false
                };
            }

            try
            {
                var doDeleteOriginal = SafeParser.ToBoolean(_configuration[SettingRegistry.ProcessingDoDeleteOriginal]);
                var argument = $" -d \"{fileSystemDirectoryInfo.Path}\" -r {(doDeleteOriginal ? "0" : "1")}";
                await $"bash {script}{argument}".Bash();
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
