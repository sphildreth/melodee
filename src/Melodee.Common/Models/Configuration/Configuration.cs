using System.Text.Json.Serialization;
using Melodee.Common.Models.Extensions;

namespace Melodee.Common.Models.Configuration;

[Serializable]
public sealed record Configuration
{
    public string? Environment { get; init; }
    
    /// <summary>
    /// Files in this directory are scanned and Release information is gathered.
    /// </summary>
    public string InboundDirectory { get; init; } = null!;

    /// <summary>
    /// When the user is happy with scan results the user can select Releases (and all associated files) to move to this directory in the proper directory structure.
    /// </summary>
    public string StagingDirectory { get; init; } = null!;

    [JsonIgnore] 
    public FileSystemDirectoryInfo StagingDirectoryInfo => new System.IO.DirectoryInfo(StagingDirectory).ToDirectorySystemInfo();
    
    /// <summary>
    /// This is the main storage library (holds all previously scanned/edited/approved Releases).
    /// </summary>
    public string LibraryDirectory { get; init; } = null!;

    /// <summary>
    /// Settings for scripting files.
    /// </summary>
    public Scripting Scripting { get; init; } = null!;

    /// <summary>
    /// Options for converting media plugins.
    /// </summary>
    public MediaConvertorOptions MediaConvertorOptions { get; init; } = null!;

    /// <summary>
    /// Options for all plugins.
    /// </summary>
    public PluginProcessOptions PluginProcessOptions { get; init; }= null!;
}
