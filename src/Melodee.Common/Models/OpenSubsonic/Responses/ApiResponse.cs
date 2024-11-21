namespace Melodee.Common.Models.OpenSubsonic.Responses;

public record ApiResponse
{
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// Used for the collection of data elements to return, e.g. "albumList2"
    /// </summary>
    public required string DataPropertyName { get; init; }
    
    /// <summary>
    /// Used for the type of element in the collection of data to return, e.g. "album". Leave null to omit.
    /// </summary>
    public string? DataDetailPropertyName { get; init; }
    
    public required string Status { get; init; }
    
    public required string Version { get; init; }
    
    public required string Type { get; init; }
    
    public required string ServerVersion { get; init; }
    
    public Error? Error { get; init; }
    
    public object? Data { get; init; }
}
