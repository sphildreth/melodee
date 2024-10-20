using Melodee.Common.Enums;
using Melodee.Common.Extensions;

namespace Melodee.Common.Models.Cards;

[Serializable]
public record ReleaseCard
{
    public bool IsValid { get; init; }

    public required long UniqueId { get; init; }

    /// <summary>
    ///     What plugins were utilized in discovering this release.
    /// </summary>
    public required IEnumerable<string> ViaPlugins { get; init; }

    public string? Artist { get; init; }

    public string? Title { get; init; }

    public int? Year { get; init; }

    public int TrackCount { get; init; }

    public required ReleaseStatus ReleaseStatus { get; init; }

    public string? Duration { get; init; }

    public byte[]? ImageBytes { get; init; }

    public required string Directory { get; init; }

    public required DateTimeOffset Created { get; init; }

    public string InfoLine => $"{Year} | {TrackCount.ToStringPadLeft(3)} | {Duration}";
}
