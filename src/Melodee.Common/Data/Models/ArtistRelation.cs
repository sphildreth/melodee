using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(ArtistId), nameof(RelatedArtistId), IsUnique = true)]
public class ArtistRelation : DataModelBase
{
    [RequiredGreaterThanZero] public required int ArtistId { get; set; }

    public Artist Artist { get; set; } = null!;

    [RequiredGreaterThanZero] public required int RelatedArtistId { get; set; }

    public Artist RelatedArtist { get; set; } = null!;

    [RequiredGreaterThanZero] public int ArtistRelationType { get; set; }

    [NotMapped] public ArtistRelationType ArtistRelationTypeValue => SafeParser.ToEnum<ArtistRelationType>(ArtistRelationType);

    public Instant? RelationStart { get; set; }

    public Instant? RelationEnd { get; set; }
}
