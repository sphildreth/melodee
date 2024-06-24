using System.Diagnostics;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Plugins.Conversion.Models;
using Serilog;
using DirectoryInfo = System.IO.DirectoryInfo;

namespace Melodee.Plugins.Scripting;

public sealed class PreDiscoveryScript(Configuration configuration)  : IScriptPlugin
{
    private readonly Configuration _configuration = configuration;
    
    public string Id => "837CE3BD-F854-4B8D-A64A-978649AAB08A";
    
    public string DisplayName => nameof(PreDiscoveryScript);

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;

    public bool StopProcessing { get; } = false;
    
    public async Task<OperationResult<bool>> ProcessAsync(DirectoryInfo directoryInfo, ProcessFileOptions processFileOptions, CancellationToken cancellationToken = default)
    {
        var result = false;
        string? scriptOutput = null;
        if (_configuration.Scripting.IsEnabled && !string.IsNullOrWhiteSpace(_configuration.Scripting.PreDiscoveryScript))
        {
            try
            {
                // Process p2 = new Process();
                // p2.StartInfo.UseShellExecute = true;
                // p2.StartInfo.FileName = "/usr/bin/chromium";
                // p2.StartInfo.Arguments = "www.sphildreth.com";
                // p2.Start();                
                
                var argument = $"{_configuration.Scripting.PreDiscoveryScript} --directory '{directoryInfo.FullName}'";
                if (!processFileOptions.DoDeleteOriginal)
                {
                    argument += $" --readonly";
                }
                
                var info = new ProcessStartInfo
                {
                    FileName = _configuration.Scripting.ScriptExecutableFullName,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Arguments = argument
                };
                using var process = Process.Start(info);
                if (process != null)
                {
                    using var reader = process.StandardOutput;
                    scriptOutput = await reader.ReadToEndAsync(cancellationToken);
                    await process.WaitForExitAsync(cancellationToken);                
                }
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
                {"output", scriptOutput ?? string.Empty}
            },
            Data = result
        };
    }
}