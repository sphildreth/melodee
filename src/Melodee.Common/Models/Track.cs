using System.Diagnostics;
using System.Text.Json.Serialization;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models;

[Serializable]
public sealed record Track
{
    //public long UniqueId => SafeParser.Hash(this.Artist(), this.ReleaseYear().ToString(), this.ReleaseTitle());
    public long ReleaseUniqueId 
    {
        get
        {
            try
            {
                return SafeParser.Hash(this.ReleaseArtist(), this.ReleaseYear().ToString(), this.ReleaseTitle());
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }

            return 0;
        }
    }

    /// <summary>
    ///     Unique TrackId on Release
    /// </summary>
    public long TrackId
    {
        get
        {
            try
            {
                return SafeParser.Hash(this.MediaNumber().ToString(), this.TrackNumber().ToString(), this.Title());
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
            return 0;
        }
    }

    /// <summary>
    ///     Globally UnqiueId
    /// </summary>
    public long UniqueId
    {
        get
        {
            try
            {
                return SafeParser.Hash(ReleaseUniqueId.ToString(), this.TrackArtist(), this.TrackYear().ToString(), this.MediaNumber().ToString(), this.TrackNumber().ToString(), this.Title());                
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

    public string DisplaySummary => $"{this.MediaNumber().ToStringPadLeft(2)}/{this.MediaTotalNumber().ToStringPadLeft(2)} : {this.TrackNumber().ToStringPadLeft(3)}/{this.TrackTotalNumber().ToStringPadLeft(3)} : {this.Title()}";

    public override string ToString()
    {
        return $"ReleaseId [{ReleaseUniqueId}] TrackId [{UniqueId}] File [{File}]";
    }
}
