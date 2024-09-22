namespace Melodee.Plugins.Validation.Models;

public sealed record ValidationResultMessage
{
    public required string Message { get; init; }

    public int SortOrder { get; init; }

    public required ValidationResultMessageSeverity Severity { get; init; }
}
