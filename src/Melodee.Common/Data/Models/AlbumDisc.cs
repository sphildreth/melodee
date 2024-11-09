using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(AlbumId), nameof(DiscNumber), IsUnique = true)]
public sealed class AlbumDisc
{
    public int Id { get; set; }

    [RequiredGreaterThanZero] public int AlbumId { get; set; }

    public Album Album { get; init; } = null!;

    [RequiredGreaterThanZero] public int DiscNumber { get; set; }

    public short? SongCount { get; set; }

    /// <summary>
    ///     TSST
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? Title { get; set; }

    public ICollection<Song> Songs { get; set; } = new List<Song>();
}
