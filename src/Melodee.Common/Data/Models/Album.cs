using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
public sealed class Album : MetaDataModelBase
{
    [RequiredGreaterThanZero] public int ArtistId { get; set; }

    public Artist Artist { get; set; } = null!;

    [RequiredGreaterThanZero] public long MediaUniqueId { get; set; }

    public short AlbumStatus { get; set; }

    [NotMapped] public AlbumStatus AlbumStatusValue => SafeParser.ToEnum<AlbumStatus>(AlbumStatus);

    public short AlbumType { get; set; }

    [NotMapped] public AlbumType AlbumTypeValue => SafeParser.ToEnum<AlbumType>(AlbumType);

    public LocalDate OriginalReleaseDate { get; set; }

    public LocalDate ReleaseDate { get; set; }

    public bool IsCompilation { get; set; }

    public short? SongCount { get; set; }

    public short? DiscCount { get; set; }

    public int Duration { get; set; }

    /// <summary>
    ///     Pipe seperated list. These are strictly defined in the Genre enum.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? Genres { get; set; }

    public ICollection<Contributor> Contributors { get; set; } = new List<Contributor>();

    public ICollection<AlbumDisc> Discs { get; set; } = new List<AlbumDisc>();

    public ICollection<UserAlbum> UserAlbums { get; set; } = new List<UserAlbum>();
}
