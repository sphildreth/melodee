namespace Melodee.Common.Data.Models.DTOs;

/// <summary>
/// Used to populate Artist index "GetIndexes" response.
/// </summary>
public record DatabaseArtistIndexInfo(Guid ApiKey, string Index, string Name, string CoverArt, decimal CalculatedRating);
