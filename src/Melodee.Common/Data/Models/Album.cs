using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Melodee.Common.Data.Contants;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Index(nameof(ApiKey), IsUnique = true)]
public sealed class Album
{
    public int Id { get; set; }
    
    public int ArtistId { get; set; }
    
    public bool IsLocked { get; set; }
    
    public short Status { get; set; }
    
    [NotMapped]
    public AlbumStatus StatusValue => SafeParser.ToEnum<AlbumStatus>(Status);
    
    public short Type { get; set; }
    
    [NotMapped]
    public AlbumType TypeValue => SafeParser.ToEnum<AlbumType>(Type);

    public Guid ApiKey { get; set; } = Guid.NewGuid();
    
    public required Instant CreatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();
    
    public Instant? LastUpdatedAt { get; set; }
    
    public Instant? LastPlayedAt { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string Name { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public string? AlternateNames { get; set; }
    
    public LocalDate OriginalReleaseDate { get; set; }
    
    public LocalDate ReleaseDate { get; set; }
    
    public bool IsCompilation { get; set; }
    
    public short? SongCount { get; set; }
    
    public short? DiscCount { get; set; }
    
    public short? PlayCount { get; set; }
    
    public string? MusicBrainzId { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? Tags { get; set; }
    
    public int Duration { get; set; }

    public ICollection<AlbumDisc> Discs { get; set; } = new List<AlbumDisc>();
}
