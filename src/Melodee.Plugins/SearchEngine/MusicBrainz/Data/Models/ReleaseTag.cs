using Melodee.Common.Extensions;
using ServiceStack.DataAnnotations;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record ReleaseTag
{
    [Ignore] public bool IsValid => ReleaseId > 0 && TagId > 0;

    public long Id { get; init; }
    
    [Index(Unique = false)] public long ReleaseId { get; init; }
    
    [Index(Unique = false)] public long TagId { get; init; }
}
