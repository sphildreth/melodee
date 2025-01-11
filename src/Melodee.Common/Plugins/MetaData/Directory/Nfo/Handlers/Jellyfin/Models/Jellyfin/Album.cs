namespace Melodee.Common.Plugins.MetaData.Directory.Nfo.Handlers.Jellyfin.Models.Jellyfin;

public class Album
{
    public string? Review { get; set; }
    public string? Outline { get; set; }
    public bool? LockData { get; set; }
    public DateTime? DateAdded { get; set; }
    public string? Title { get; set; }
    public int? Year { get; set; }
    public DateTime? Premiered { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public int? Runtime { get; set; }
    public string? Genre { get; set; }
    public string? AudioDbArtistId { get; set; }
    public string? AudioDbAlbumId { get; set; }
    public string? MusicBrainzAlbumId { get; set; }
    public string? MusicBrainzAlbumArtistId { get; set; }
    public string? MusicBrainzReleaseGroupId { get; set; }
    public List<Art>? Art { get; set; }
    public List<Actor>? Actor { get; set; }
    public List<string>? Artist { get; set; }
    public string? AlbumArtist { get; set; }
    public List<Track>? Track { get; set; }
}
