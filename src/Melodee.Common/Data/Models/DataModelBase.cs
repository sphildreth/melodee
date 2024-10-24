using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Index(nameof(ApiKey), IsUnique = true)]
public abstract class DataModelBase
{
    public int Id { get; set; }

    /// <summary>
    /// Automatically updated to the PostgresSQL row-version value when a record is updated.
    /// </summary>
    [Timestamp]
    public byte[] Version { get; set; } = null!;    
    
    public bool IsLocked { get; set; }
    
    public int SortOrder { get; set; }
 
    [Required]
    public Guid ApiKey { get; set; } = Guid.NewGuid();
    
    [Required]
    public required Instant CreatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();
    
    public Instant? LastUpdatedAt { get; set; }
    
    /// <summary>
    /// Pipe seperated list. Example 'terrible|sexy|Songs about Love'
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? Tags { get; set; }    
    
    /// <summary>
    /// Should be markdown, will be rendered to HTML or to text depending on consumer.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Should be markdown, will be rendered to HTML or to text depending on consumer.
    /// </summary>    
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? Description { get; set; }
}
