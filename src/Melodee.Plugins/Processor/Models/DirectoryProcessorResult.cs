namespace Melodee.Plugins.Processor.Models;

public record DirectoryProcessorResult
{
    public required int NumberOfConversionPluginsProcessed { get; init; }

    public required int NumberOfDirectoryPluginProcessed { get; init; }

    public required int NumberOfAlbumFilesProcessed { get; init; }

    public required int NewArtistsCount { get; init; }

    public required int NewAlbumsCount { get; init; }

    public required int NewSongsCount { get; init; }

    public required double DurationInMs { get; init; }

    public required int NumberOfValidAlbumsProcessed { get; init; }

    public required int NumberOfConversionPluginsProcessedFileCount { get; init; }
}
