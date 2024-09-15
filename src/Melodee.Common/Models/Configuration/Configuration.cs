using System.Text.Json.Serialization;
using Melodee.Common.Models.Extensions;

namespace Melodee.Common.Models.Configuration;

[Serializable]
public sealed record Configuration
{
    public string? Environment { get; set; }
    
    /// <summary>
    /// Files in this directory are scanned and Release information is gathered.
    /// </summary>
    public string InboundDirectory { get; set; } = null!;

    /// <summary>
    /// When the user is happy with scan results the user can select Releases (and all associated files) to move to this directory in the proper directory structure.
    /// </summary>
    public string StagingDirectory { get; set; } = null!;

    public int FilterLessThanTrackCount { get; set; } = 3;
    
    public double FilterLessThanConfiguredDuration { get; set; } = 720000;
    
    public TimeSpan FilterLessThanConfiguredTime => TimeSpan.FromMilliseconds(FilterLessThanConfiguredDuration);

    [JsonIgnore] 
    public FileSystemDirectoryInfo StagingDirectoryInfo => new System.IO.DirectoryInfo(StagingDirectory).ToDirectorySystemInfo();
    
    public int StagingDirectoryScanLimit { get; set; } = 250;
    
    /// <summary>
    /// This is the main storage library (holds all previously scanned/edited/approved Releases).
    /// </summary>
    public string LibraryDirectory { get; set; } = null!;

    /// <summary>
    /// Settings for scripting files.
    /// </summary>
    public Scripting Scripting { get; set; } = new();

    /// <summary>
    /// Options for converting media plugins.
    /// </summary>
    public MediaConvertorOptions MediaConvertorOptions { get; set; } = new();
    
    /// <summary>
    /// Options for all plugins.
    /// </summary>
    public PluginProcessOptions PluginProcessOptions { get; set; }= new();

    /// <summary>
    /// Options for validations.
    /// </summary>
    public ValidationOptions ValidationOptions { get; set; } = new();

    /// <summary>
    /// Default page size when view including pagination.
    /// </summary>
    public short DefaultPageSize { get; set; } = 100;
}
