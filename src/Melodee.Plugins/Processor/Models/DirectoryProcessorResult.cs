namespace Melodee.Plugins.Processor.Models;

public record DirectoryProcessorResult
{
   public required int NumberOfConversionPluginsProcessed { get; init; }
   
   public required int NumberOfDirectoryPluginProcessed { get; init; }
   
   public required int NumberOfAlbumFilesProcessed { get; init; }
}
