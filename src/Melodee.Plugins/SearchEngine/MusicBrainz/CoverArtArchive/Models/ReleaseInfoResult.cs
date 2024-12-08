namespace Melodee.Plugins.SearchEngine.MusicBrainz.CoverArtArchive.Models;

public sealed record ReleaseInfoResult(ReleaseImageInfoResult[] Images, string? Release);
