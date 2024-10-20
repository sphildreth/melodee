using System.Text.Json.Serialization;
using Melodee.Common.Models.Extensions;

namespace Melodee.Common.Models.Configuration;

[Serializable]
public sealed record Configuration
{
    public string? Environment { get; set; }

    /// <summary>
    ///     Files in this directory are scanned and Album information is gathered.
    /// </summary>
    public string InboundDirectory { get; set; } = null!;
    
    [JsonIgnore] public FileSystemDirectoryInfo InboundDirectoryInfo => new DirectoryInfo(InboundDirectory).ToDirectorySystemInfo();    

    /// <summary>
    ///     When the user is happy with scan results the user can select Albums (and all associated files) to move to this
    ///     directory in the proper directory structure.
    /// </summary>
    public string StagingDirectory { get; set; } = null!;
    
    [JsonIgnore] public FileSystemDirectoryInfo StagingDirectoryInfo => new DirectoryInfo(StagingDirectory).ToDirectorySystemInfo();    

    public int FilterLessThanSongCount { get; set; } = 3;

    public double FilterLessThanConfiguredDuration { get; set; } = 720000;

    public TimeSpan FilterLessThanConfiguredTime => TimeSpan.FromMilliseconds(FilterLessThanConfiguredDuration);

    public int StagingDirectoryScanLimit { get; set; } = 250;

    /// <summary>
    ///     This is the main storage library (holds all previously scanned/edited/approved Albums).
    /// </summary>
    public string LibraryDirectory { get; set; } = null!;
    
    [JsonIgnore] public FileSystemDirectoryInfo LibraryDirectoryInfo => new DirectoryInfo(LibraryDirectory).ToDirectorySystemInfo();

    /// <summary>
    ///     Settings for scripting files.
    /// </summary>
    public Scripting Scripting { get; set; } = new();

    /// <summary>
    ///     Options for converting media plugins.
    /// </summary>
    public MediaConvertorOptions MediaConvertorOptions { get; set; } = new();

    /// <summary>
    ///     Options for all plugins.
    /// </summary>
    public PluginProcessOptions PluginProcessOptions { get; set; } = new();

    /// <summary>
    ///     Options for validations.
    /// </summary>
    public ValidationOptions ValidationOptions { get; set; } = new();
    
    /// <summary>
    ///     Options for doing magic. 
    /// </summary>
    public MagicOptions MagicOptions { get; set; } = new();

    /// <summary>
    ///     Default page size when view including pagination.
    /// </summary>
    public short DefaultPageSize { get; set; } = 100;

    /// <summary>
    ///     When true then move the Melodee.json data file when moving Albums, otherwise delete.
    /// </summary>
    public bool MoveMelodeeJsonDataFileToLibrary { get; set; } = false;

    /// <summary>
    ///     Amount of time to display a Toast then auto-close.
    /// </summary>
    public int ToastAutoCloseTime { get; set; } = 2000;

    /// <summary>
    ///     Short Format to use when displaying full dates.
    /// </summary>
    public string DateTimeDisplayFormatShort { get; set; } = "yyyyMMdd HH:mm";
    
    /// <summary>
    ///     Format to use when displaying activity related dates (e.g. processing messages)
    /// </summary>
    public string DateTimeDisplayActivityFormat { get; set; } = "HH:mm:ss.fff";
}
