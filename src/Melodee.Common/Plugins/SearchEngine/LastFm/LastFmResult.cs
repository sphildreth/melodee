namespace Melodee.Common.Plugins.SearchEngine.LastFm;

public record LastFmResult(
    Results? Results
);

public record Results(
    Opensearch_Query? OpensearchQuery,
    string? OpensearchTotalResults,
    string? OpensearchStartIndex,
    string? OpensearchItemsPerPage,
    Albummatches? Albummatches
);

public record Opensearch_Query(
    string? _text,
    string? role,
    string? searchTerms,
    string? startPage
);

public record Albummatches(
    Album[]? album
);

public record Album(
    string? name,
    string? artist,
    string? url,
    Image[]? image,
    string? streamable,
    string? mbid
);

public record Image(
    string? _text,
    string? size
);
