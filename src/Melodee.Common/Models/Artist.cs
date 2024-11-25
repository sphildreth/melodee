namespace Melodee.Common.Models;

public record Artist(string Name, string NameNormalized, string? SortName, string? MusicBrainzId = null);
