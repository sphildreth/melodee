using System.Text;

namespace Melodee.Common.Models.OpenSubsonic.Extensions;

public static class LyricsListExtensions
{
    public static Lyrics ToLyrics(this LyricsList lyricsList)
    {
        var stringBuilder = new StringBuilder();
        foreach (var line in lyricsList.Line)
        {
            stringBuilder.Append($"{line.Value}\n");
        }
        return new Lyrics
        {
            Artist = lyricsList.DisplayArtist ?? throw new Exception("Artist is required"),
            Title = lyricsList.DisplayTitle  ?? throw new Exception("Title is required"),
            Value = stringBuilder.ToString()
        };
    }
}
