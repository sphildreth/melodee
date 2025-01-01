using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(UserId), nameof(SongId), IsUnique = true)]
public class Bookmark : MetaDataModelBase
{
    [RequiredGreaterThanZero] public required int UserId { get; set; }

    public User User { get; set; } = null!;

    [RequiredGreaterThanZero] public required int SongId { get; set; }

    public Song Song { get; set; } = null!;

    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public string? Comment { get; set; }

    public int Position { get; set; }
}
