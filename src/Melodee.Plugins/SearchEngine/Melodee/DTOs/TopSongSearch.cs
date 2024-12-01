using Melodee.Common.Data.Models;

namespace Melodee.Plugins.SearchEngine.Melodee.DTOs;

[Serializable]
public class TopSongSearch : Song
{
    public int Index { get; set; }
}
