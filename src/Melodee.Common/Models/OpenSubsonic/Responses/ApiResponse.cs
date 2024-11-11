namespace Melodee.Common.Models.OpenSubsonic.Responses;

public record ApiResponse(
    string status,
    string version,
    string type,
    string serverVersion,
    bool openSubsonic,
    Error? error
);
