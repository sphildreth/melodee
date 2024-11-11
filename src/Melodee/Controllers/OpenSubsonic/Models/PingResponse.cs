namespace Melodee.Controllers.OpenSubsonic.Models;

public record PingResponse(
    string status,
    string version,
    string type,
    string serverVersion,
    bool openSubsonic,
    Error? error
);
