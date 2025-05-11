namespace Melodee.Common.Models.SearchEngines;

public sealed record ArtistSearchInfoResult(
    Guid? ApiKey,
    string Name,
    string SortName,
    string? ImageUrl,
    Guid? MusicBrainzId);
