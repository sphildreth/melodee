using Melodee.Common.Enums;
using Melodee.Common.Extensions;

namespace Melodee.Common.Models.Cards;

[Serializable]
public record AlbumCard
{
    public bool IsValid { get; init; }

    public required long UniqueId { get; init; }

    /// <summary>
    ///     What plugins were utilized in discovering this Album.
    /// </summary>
    public required IEnumerable<string> ViaPlugins { get; init; }

    public string? Artist { get; init; }

    public string? Title { get; init; }

    public int? Year { get; init; }

    public int SongCount { get; init; }

    public required AlbumStatus AlbumStatus { get; init; }

    public string? Duration { get; init; }

    public byte[]? ImageBytes { get; init; }

    public required string Directory { get; init; }

    public required DateTimeOffset Created { get; init; }

    public string InfoLine => $"{Year} | {SongCount.ToStringPadLeft(3)} | {Duration}";
}
