namespace Melodee.Common.Models.OpenSubsonic.Responses;

public record ApiResponse
{
    public bool IsSuccess { get; init; }
    
    public required string DataPropertyName { get; init; }
    
    public required string Status { get; init; }
    
    public required string Version { get; init; }
    
    public required string Type { get; init; }
    
    public required string ServerVersion { get; init; }
    
    public Error? Error { get; init; }
    
    public object? Data { get; init; }
}
