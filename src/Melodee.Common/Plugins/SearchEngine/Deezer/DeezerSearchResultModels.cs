namespace Melodee.Common.Plugins.SearchEngine.Deezer;

public record DeezerSongSearchResult(
    Song[] Data,
    int Total
);

public record Song(
    int Id,
    bool Readable,
    string Title,
    string Title_Short,
    string Title_Version,
    string Link,
    int Duration,
    int Rank,
    bool Explicit_Lyrics,
    int Explicit_Content_Lyrics,
    int Explicit_Content_Cover,
    string Preview,
    string Md5_Image,
    Artist Artist,
    Album Album,
    string Type
);
