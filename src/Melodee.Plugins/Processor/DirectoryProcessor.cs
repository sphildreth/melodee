using System.Runtime;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.Conversion;
using Melodee.Plugins.Conversion.Image;
using Melodee.Plugins.Conversion.Media;
using Melodee.Plugins.Discovery.Releases;
using Melodee.Plugins.Scripting;
using SerilogTimings;
using DirectoryInfo = Melodee.Common.Models.DirectoryInfo;

namespace Melodee.Plugins.Processor;

/// <summary>
/// Take a given directory and process all the directories in it. 
/// </summary>
public sealed class DirectoryProcessor : IProcessorPlugin
{
    private readonly Configuration _configuration;
    private readonly IScriptPlugin _preDiscoveryScript;
    private readonly IReleasesDiscoverer _releasesDiscoverer;
    private readonly IEnumerable<IConversionPlugin> _enabledConversionPlugins;
    
    public string Id => "9BF95E5A-2EB5-4E28-820A-6F3B857356BD";

    public string DisplayName => nameof(DirectoryProcessor);

    public bool IsEnabled { get; set; } = true;
    
    public int SortOrder { get; } = 0;

    public DirectoryProcessor(Configuration configuration)
    {
        _configuration = configuration;
        
        _preDiscoveryScript = new PreDiscoveryScript(_configuration);
        _releasesDiscoverer = new ReleasesDiscoverer(_configuration);

        _enabledConversionPlugins = new List<IConversionPlugin>
        {
            new ImageConvertor(_configuration),
            new MediaConvertor(_configuration)
        };
    }
    
    public async Task<OperationResult<bool>> ProcessDirectoryAsync(DirectoryInfo directoryInfo, CancellationToken cancellationToken = default)
    {
        // Ensure directory to process exists
        var dirInfo = new System.IO.DirectoryInfo(directoryInfo.Path);
        if (!dirInfo.Exists)
        {
            return new OperationResult<bool>
            {
                Errors = new[]
                {
                    new Exception($"Directory [{directoryInfo}] not found.")
                },
                Data = false
            };
        }
        
        // Ensure that staging directory exists
        var stagingInfo = new System.IO.DirectoryInfo(_configuration.StagingDirectory);
        if (!stagingInfo.Exists)
        {
            return new OperationResult<bool>
            {
                Errors = new[]
                {
                    new Exception($"Staging Directory [{_configuration.StagingDirectory}] not found.")
                },
                Data = false
            };
        }
        
        // Run PreDiscovery script
        if (_preDiscoveryScript.IsEnabled)
        {
            var preDiscoveryScriptResult = await _preDiscoveryScript.ProcessAsync(directoryInfo, cancellationToken);
            if (!preDiscoveryScriptResult.IsSuccess)
            {
                return new OperationResult<bool>(preDiscoveryScriptResult.Messages)
                {
                    Errors = preDiscoveryScriptResult.Errors,
                    Data = false
                };
            }
        }

        var allFilesInDirectory = dirInfo.EnumerateFileSystemInfos("*.*", SearchOption.AllDirectories).ToArray();
        
        // Run Enabled Conversion scripts on each file in directory
        foreach (var fileSystemInfo in allFilesInDirectory)
        {
            foreach (var plugin in _enabledConversionPlugins)
            {
                if (plugin.DoesHandleFile(fileSystemInfo))
                {
                    using (Operation.Time("Conversion: File [{File}] Plugin [{Plugin}]", fileSystemInfo.Name, plugin.DisplayName))
                    {
                        var pluginResult = await plugin.ProcessFileAsync(fileSystemInfo, cancellationToken);
                        if (!pluginResult.IsSuccess)
                        {
                            return new OperationResult<bool>(pluginResult.Messages)
                            {
                                Errors = pluginResult.Errors,
                                Data = false
                            };
                        }
                    }
                }
                if (plugin.StopProcessing)
                {
                    break;
                }
                
            }
        }

        // Find releases in given directory
        var allReleasesInFolder = await _releasesDiscoverer.ReleasesForDirectoryAsync(directoryInfo, new PagedRequest(), cancellationToken);
        if (!allReleasesInFolder.IsSuccess)
        {
            return new OperationResult<bool>(allReleasesInFolder.Messages)
            {
                Errors = allReleasesInFolder.Errors,
                Data = false
            };
        }
        
        // Create directory and move files for each found release in staging folder
        foreach (var release in allReleasesInFolder.Data)
        {
            var fullReleaseDirectoryName = release.ReleaseDirectoryName(_configuration);
            var releaseDirInfo = new System.IO.DirectoryInfo(Path.Combine(_configuration.StagingDirectory, fullReleaseDirectoryName));
            if (!releaseDirInfo.Exists)
            {
                releaseDirInfo.Create();
            }
            foreach (var track in release.Tracks)
            {
                var newTrackFileName = Path.Combine(releaseDirInfo.FullName, track.TrackFileName(_configuration));
                if (_configuration.PluginProcessOptions.DoDeleteOriginal)
                {
                    File.Move(track.FileSystemInfo.FullName, newTrackFileName);
                }
                else
                {
                    File.Copy(track.FileSystemInfo.FullName, newTrackFileName);
                }
            }
            
            // foreach (var file in allFilesInDirectory)
            // {
            //     if (release.IsFileForRelease(file) && FileHelper.IsFileImageType(file.FullName))
            //     {
            //         var newFileName = new System.IO.FileInfo(Path.Combine(releaseDirInfo.FullName, file.Name));
            //     }
            // }
        }

        return new OperationResult<bool>
        {
            Data = true
        };
    }
}