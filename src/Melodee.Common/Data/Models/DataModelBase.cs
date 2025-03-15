using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(ApiKey), IsUnique = true)]
public abstract class DataModelBase
{
    public int Id { get; set; }

    public bool IsLocked { get; set; }

    public int SortOrder { get; set; }

    [Required] public Guid ApiKey { get; set; } = Guid.NewGuid();

    [Required] public required Instant CreatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();

    public Instant? LastUpdatedAt { get; set; }

    /// <summary>
    ///     Pipe seperated list. Example 'terrible|sexy|Songs about Love'
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? Tags { get; set; }

    /// <summary>
    ///     Should be markdown, will be rendered to HTML or to text depending on consumer.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? Notes { get; set; }

    /// <summary>
    ///     Stored in markdown, will be rendered to HTML or to text depending on consumer.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxTextLength)]
    public string? Description { get; set; }
}
