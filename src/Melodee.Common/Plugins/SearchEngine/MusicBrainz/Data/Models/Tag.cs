using Melodee.Common.Extensions;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models;

public sealed record Tag
{
    public bool IsValid => Id > 0 && Name.Nullify() != null;

    public long Id { get; init; }

    public required string Name { get; init; }
}
