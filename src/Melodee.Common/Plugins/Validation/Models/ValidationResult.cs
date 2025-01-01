using Melodee.Common.Models.Validation;

namespace Melodee.Common.Plugins.Validation.Models;

public record ValidationResult
{
    public bool IsValid { get; init; }

    public IEnumerable<ValidationResultMessage>? Messages { get; init; }
}
