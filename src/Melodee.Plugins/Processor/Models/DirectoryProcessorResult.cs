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

    public required int NumberOfAlbumsProcessed { get; init; }
    
    public required int NumberOfValidAlbumsProcessed { get; init; }

    public string FormattedValidPercentageProcessed => $"{((int)Math.Round((double)(100*NumberOfValidAlbumsProcessed) / NumberOfAlbumsProcessed))}%";

    public string SuccessSummary => $"Processed [{NumberOfAlbumsProcessed}] albums and {FormattedValidPercentageProcessed} [{NumberOfValidAlbumsProcessed}] are [Ok] status.";
    
    public required int NumberOfConversionPluginsProcessedFileCount { get; init; }
}
