namespace Melodee.Common.Models.Configuration;

[Serializable]
public sealed record Configuration
{
    /// <summary>
    /// Files in this folder are scanned and Release information is gathered.
    /// </summary>
    public required string InboundDirectory { get; init; }
    
    /// <summary>
    /// When the user is happy with scan results the user can select Releases (and all associated files) to move to this directory in the proper folder structure.
    /// </summary>
    public required string StagingDirectory { get; init; }
    
    /// <summary>
    /// This is the main storage library (holds all previously scanned/edited/approved Releases).
    /// </summary>
    public required string LibraryDirectory { get; init; }
    
    /// <summary>
    /// Settings for scripting files.
    /// </summary>
    public required Scripting Scripting { get; init; }
    
    /// <summary>
    /// Options for converting media plugins.
    /// </summary>
    public required MediaConvertorOptions MediaConvertorOptions { get; init; }
    
    /// <summary>
    /// Options for all plugins.
    /// </summary>
    public required PluginProcessOptions PluginProcessOptions { get; init; }
}