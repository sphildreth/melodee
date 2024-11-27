using Melodee.Common.Extensions;
using ServiceStack.DataAnnotations;

namespace Melodee.Services.MusicBrainz.Data.Models;

public sealed record ArtistAlias
{
    [Ignore]
    public bool IsValid => ArtistId > 0 && Name.Nullify() != null;

    public long Id { get; init; }

    [Index(Unique = false)]
    public required long ArtistId { get; init; }

    public int Type { get; init; }

    public required string Name { get; init; }
    
    public string? SortName { get; init; }
}
