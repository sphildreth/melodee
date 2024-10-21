using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Melodee.Common.Data.Contants;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using NodaTime;

namespace Melodee.Common.Data.Models;

public sealed class Album : MetaDataModelBase
{
    public int ArtistId { get; set; }
    
    public short AlbumStatus { get; set; }
    
    [NotMapped]
    public AlbumStatus AlbumStatusValue => SafeParser.ToEnum<AlbumStatus>(AlbumStatus);
    
    public short AlbumType { get; set; }
    
    [NotMapped]
    public AlbumType AlbumTypeValue => SafeParser.ToEnum<AlbumType>(AlbumType);
    
    public LocalDate OriginalReleaseDate { get; set; }
    
    public LocalDate ReleaseDate { get; set; }
    
    public bool IsCompilation { get; set; }
    
    public short? SongCount { get; set; }
    
    public short? DiscCount { get; set; }
    
    public int Duration { get; set; }
    
    /// <summary>
    /// Pipe seperated list. These are strictly defined in the Genre enum.
    /// </summary>
    public string? Genres { get; set; }

    public ICollection<AlbumDisc> Discs { get; set; } = new List<AlbumDisc>();
}
