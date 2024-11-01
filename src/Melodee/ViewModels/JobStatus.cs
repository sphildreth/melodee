namespace Melodee.ViewModels;

public sealed record JobStatus(string Group, string Name, string TriggerName, string TriggerGroup, string TriggerType, string TriggerState, string? NextFireTime, string? PreviousFireTime);
