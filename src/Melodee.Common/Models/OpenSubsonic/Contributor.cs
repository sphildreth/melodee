namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
///     A contributor artist for a song or an album.
/// </summary>
/// <param name="Role">The contributor role.</param>
/// <param name="ArtistId3">
///     The subRole for roles that may require it. Ex: The instrument for the performer role
///     (TMCL/performer tags). Note: For consistency between different tag formats, the TIPL sub roles should be directly
///     exposed in the role field.
/// </param>
/// <param name="SubRole">
///     The artist taking on the role. (Note: Only the required ArtistID3 fields should be returned by
///     default)
/// </param>
public record Contributor(
    string Role,
    ArtistID3 ArtistId3,
    string? SubRole
);
