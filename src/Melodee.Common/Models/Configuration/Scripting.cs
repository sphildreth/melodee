namespace Melodee.Common.Models.Configuration;

[Serializable]
public sealed record Scripting
{
    public bool IsEnabled => !string.IsNullOrWhiteSpace(ScriptExecutableFullName);
    
    public string? ScriptExecutableFullName { get; init; } = "python";
    
    public string? PreDiscoveryScript { get; init; }
    
    public string? PostDiscoveryScript { get; init; }
}