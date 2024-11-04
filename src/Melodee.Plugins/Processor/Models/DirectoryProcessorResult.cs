namespace Melodee.Plugins.Processor.Models;

public record DirectoryProcessorResult
{
   public required int NumberOfConversionPluginsProcessed { get; init; }
   
   public required int NumberOfDirectoryPluginProcessed { get; init; }
   
   public required int NumberOfAlbumFilesProcessed { get; init; }
   
   public required int NewArtistsCount { get; set; }
    
   public required int NewAlbumsCount { get; set; }
    
   public required int NewSongsCount { get; set; }
    
   public required double DurationInMs { get; set; }   
}
