namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
///     A list operation of Artists
///     <remarks>
///         See https://opensubsonic.netlify.app/docs/responses/artists/
///     </remarks>
/// </summary>
public sealed record ArtistList(
    string IgnoredArticles,
    ArtistIndex Index
);
