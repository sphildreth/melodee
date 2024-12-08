namespace Melodee.Plugins.SearchEngine.MusicBrainz.CoverArtArchive.Models;

public sealed record ReleaseImageInfoResult(
    bool Approved,
    bool Back,
    string? Comment,
    long Edit,
    bool Front,
    long Id,
    string Image,
    string[]? Types,
    Dictionary<string, string> Thumbnails);
