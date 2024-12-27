using Melodee.Common.Extensions;

namespace Melodee.Common.Models.SpecialArtists;

/// <summary>
///     Used for compilation-type albums containing songs by multiple different artists.
///     <remarks>
///         See https://musicbrainz.org/artist/89ad4ac3-39f7-470e-963a-56509c546377
///         SoundTracks who have a single writer should use that writer as their artist.
///         SoundTracks who have multiple writers should use this SpecialArtist. 
///         This artist shouldn't generally be used for album or songs.
///     </remarks>
/// </summary>
public sealed record VariousArtist() : Artist(
    "Various Artist",
    "Various Artist".ToNormalizedString()!,
    "Various Artist",
    null,
    null,
    "89ad4ac3-39f7-470e-963a-56509c546377");
