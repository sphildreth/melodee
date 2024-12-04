using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Validators;

namespace Melodee.Common.Data.Models;

[Serializable]
public class PlayQueue : DataModelBase
{
    [RequiredGreaterThanZero] public int UserId { get; set; }

    public User User { get; set; } = null!;

    [RequiredGreaterThanZero] public int SongId { get; set; }

    /// <summary>
    ///     This is to not expose the SongId to API consumers and allow for PlayQueue management by API consumers.
    /// </summary>
    [Required]
    public Guid SongApiKey { get; set; }

    public Song Song { get; set; } = null!;
    
    /// <summary>
    /// This is to flag if this que is the currently playing song.
    /// </summary>
    public bool IsCurrentSong { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string ChangedBy { get; set; }

    [Required] public double Position { get; set; }
    
    [Required] public int PlayQueId { get; set; }
}
