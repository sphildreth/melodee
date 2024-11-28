using Melodee.Common.Extensions;
using ServiceStack.DataAnnotations;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record Tag
{
    [Ignore] public bool IsValid => Id > 0 && Name.Nullify() != null;

    public long Id { get; init; }

    public required string Name { get; init; }
}
