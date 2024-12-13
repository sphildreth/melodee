namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record LinkType
{
    public long Id { get; init; }

    public long? ParentId { get; init; }

    public int ChildOrder { get; init; }

    public required Guid MusicBrainzId { get; init; }

    public required string EntityType0 { get; init; }

    public required string EntityType1 { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public required string LinkPhrase { get; init; }

    public required string ReverseLinkPhrase { get; init; }

    public required bool HasDates { get; init; }

    public required int Entity0Cardinality { get; init; }

    public required int Entity1Cardinality { get; init; }
}
