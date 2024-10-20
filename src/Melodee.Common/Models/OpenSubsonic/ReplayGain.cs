namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
/// The replay gain data of a song.
/// </summary>
/// <param name="TrackGain">The track replay gain value. (In Db)</param>
/// <param name="AlbumGain">The album replay gain value. (In Db)</param>
/// <param name="TrackPeak">The track peak value. (Must be positive)</param>
/// <param name="AlbumPeak">The album peak value. (Must be positive)</param>
/// <param name="BaseGain">The base gain value. (In Db) (Ogg Opus Output Gain for example)</param>
/// <param name="FallbackGain">An optional fallback gain that clients should apply when the corresponding gain value is missing. (Can be computed from the tracks or exposed as an user setting.)</param>
public record ReplayGain(
    double? TrackGain,
    double? AlbumGain,
    double? TrackPeak,
    double? AlbumPeak,
    double? BaseGain,
    double? FallbackGain
);
