using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Utility;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data.Enums;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record ReleaseGroup
{
    public long Id { get; init; }

    public long ArtistCreditId { get; init; }

    public string? Name { get; init; }

    public int ReleaseType { get; init; }

    public required string MusicBrainzIdRaw { get; init; }

    [NotMapped]
    public Guid MusicBrainzId => Guid.Parse(MusicBrainzIdRaw);

    public ReleaseType ReleaseTypeValue => SafeParser.ToEnum<ReleaseType>(ReleaseType);
}
