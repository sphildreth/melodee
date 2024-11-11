namespace Melodee.Common.Models.OpenSubsonic.Responses;

public record ApiResponse(
    string Status,
    string Version,
    string Type,
    string ServerVersion,
    bool OpenSubsonic,
    Error? Error
);
