using ServiceStack.DataAnnotations;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record ReleaseTag
{
    public bool IsValid => ReleaseId > 0 && TagId > 0;

    public long Id { get; init; }

    public long ReleaseId { get; init; }

    public long TagId { get; init; }
}
