namespace Melodee.Common.Plugins.SearchEngine.Deezer;

public record Album(
    int Id,
    string Title,
    string Cover,
    string Cover_Small,
    string Cover_Medium,
    string Cover_Big,
    string Cover_Xl,
    string Md5_Image,
    int? Genre_Id,
    int? Nb_Tracks,
    string? Record_Type,
    bool Explicit_Lyrics,
    string Tracklist,
    string Type,
    Artist? Artist
);
