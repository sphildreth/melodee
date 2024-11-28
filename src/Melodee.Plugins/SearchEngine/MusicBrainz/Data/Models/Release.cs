using Melodee.Common.Extensions;
using ServiceStack.DataAnnotations;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

/// <summary>
///     A release is a physical or digital product that contains audio, such as a CD or digital download. A release
///     includes information like the country, label, barcode, and packaging. A release can contain multiple media, such as
///     a CD, vinyl, or digital media.
/// </summary>
public sealed record Release
{
    public bool IsValid => MusicBrainzId != Guid.Empty && Name.Nullify() != null;

    public long Id { get; init; }

    public Guid MusicBrainzId { get; init; }

    public required string Name { get; init; }

    public string? NameNormalized { get; init; }
    
    public long ReleaseGroupId { get; init; }

    public string? SortName { get; init; }

    public required long ArtistCreditId { get; init; }
}
