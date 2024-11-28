using Melodee.Common.Extensions;
using ServiceStack.DataAnnotations;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record ArtistAlias
{
    public bool IsValid => ArtistId > 0 && Name.Nullify() != null;

    public long Id { get; init; }

    public required long ArtistId { get; init; }

    public int Type { get; init; }

    public required string Name { get; init; }

    public string? SortName { get; init; }
}
