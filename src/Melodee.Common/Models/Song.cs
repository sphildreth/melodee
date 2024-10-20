using System.Diagnostics;
using System.Text.Json.Serialization;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models;

[Serializable]
public sealed record Song
{
    //public long UniqueId => SafeParser.Hash(this.Artist(), this.AlbumYear().ToString(), this.AlbumTitle());
    public long AlbumUniqueId 
    {
        get
        {
            try
            {
                return SafeParser.Hash(this.AlbumArtist(), this.AlbumYear().ToString(), this.AlbumTitle());
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }

            return 0;
        }
    }

    /// <summary>
    ///     Unique SongId on Album
    /// </summary>
    public long SongId
    {
        get
        {
            try
            {
                return SafeParser.Hash(this.MediaNumber().ToString(), this.SongNumber().ToString(), this.Title());
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
            return 0;
        }
    }

    /// <summary>
    ///     Globally UniqueId
    /// </summary>
    public long UniqueId
    {
        get
        {
            try
            {
                return SafeParser.Hash(AlbumUniqueId.ToString(), this.SongArtist(), this.SongYear().ToString(), this.MediaNumber().ToString(), this.SongNumber().ToString(), this.Title());                
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
            return 0;            
        }
    }

    public required string CrcHash { get; init; }

    public required FileSystemFileInfo File { get; init; }

    [JsonIgnore] public IEnumerable<ImageInfo>? Images { get; set; }

    public IEnumerable<MetaTag<object?>>? Tags { get; init; }

    public IEnumerable<MediaAudio<object?>>? MediaAudios { get; init; }

    public int SortOrder { get; set; }

    public string DisplaySummary => $"{this.MediaNumber().ToStringPadLeft(2)}/{this.MediaTotalNumber().ToStringPadLeft(2)} : {this.SongNumber().ToStringPadLeft(3)}/{this.SongTotalNumber().ToStringPadLeft(3)} : {this.Title()}";

    public override string ToString()
    {
        return $"AlbumId [{AlbumUniqueId}] SongId [{UniqueId}] File [{File}]";
    }
}
