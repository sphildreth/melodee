namespace Melodee.Common.Plugins.SearchEngine.Deezer;

public record Artist(
    int Id,
    string Name,
    string Link,
    string Picture,
    string Picture_Small,
    string Picture_Medium,
    string Picture_Big,
    string Picture_Xl,
    bool Radio,
    int? NB_Album,
    int? NB_Fan,
    string Tracklist,
    string Type
);
