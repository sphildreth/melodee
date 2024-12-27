using Melodee.Common.Extensions;

namespace Melodee.Common.Models.SpecialArtists;

/// <summary>
///     Original Recording, Original Cast Recording should all use this Special Artist.
///     <remarks>
///         See also https://musicbrainz.org/artist/a0ef7e1d-44ff-4039-9435-7d5fefdeecc9
///         Musical theatre soundtrack albums should generally be attributed to the songwriter(s)/author(s). Only use this
///         SpecialArtist if you really cannot identify either the songwriter(s) (or in the case of songs/albums, the
///         performers, if the printed song lists so indicate them). This SpecialArtist is particularly meant for
///         unnamed ensembles (groups of unnamed performers), which in musical theatre albums are often described typically
///         as, e.g. “Company”, “Ensemble”, “Chorus”, etc., with those descriptions or other character roles applied as the
///         artist credit.
///     </remarks>
/// </summary>
public sealed record Theater() : Artist(
    "Theater",
    "Theater".ToNormalizedString()!,
    "Theater",
    null,
    null,
    "a0ef7e1d-44ff-4039-9435-7d5fefdeecc9");
