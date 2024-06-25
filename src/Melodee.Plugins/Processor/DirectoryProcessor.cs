using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Plugins.Discovery.Releases;
using Melodee.Plugins.Scripting;
using DirectoryInfo = Melodee.Common.Models.DirectoryInfo;

namespace Melodee.Plugins.Processor;

/// <summary>
/// Take a given directory and process all the directories in it. 
/// </summary>
public sealed class DirectoryProcessor(Configuration configuration, IReleasesDiscoverer iReleasesDiscoverer) : IProcessorPlugin
{
    private readonly IReleasesDiscoverer _iReleasesDiscoverer = iReleasesDiscoverer;
    
    public string Id => "9BF95E5A-2EB5-4E28-820A-6F3B857356BD";

    public string DisplayName => nameof(DirectoryProcessor);

    public bool IsEnabled { get; set; } = true;
    public int SortOrder { get; } = 0;
    
    public Task<OperationResult<bool>> ProcessDirectoryAsync(DirectoryInfo directoryInfo, CancellationToken cancellationToken = default)
    {
        // Ensure directory to process exists
        
        // Ensure that staging directory exists
        
        // Run PreDiscovery script
        
        // Run Enabled Conversion scripts on each file in directory
        
        // Find releases in given directory
        
        // Create directory and move files for each found release in staging folder
        
        throw new NotImplementedException();
    }
}