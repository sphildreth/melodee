using Melodee.Common.Enums;
using Melodee.Common.Models.Validation;

namespace Melodee.Plugins.Validation.Models;

public sealed record ValidationResult
{
    public bool IsValid { get; init; }

    public required AlbumStatus AlbumStatus { get; init; }

    public IEnumerable<ValidationResultMessage>? Messages { get; init; }
}
