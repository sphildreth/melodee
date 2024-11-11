namespace Melodee.Controllers.OpenSubsonic.Models;

public record Error(short code, string message)
{
    public static Error RequiredParameterMissingError => new Error(10, "Required parameter is missing.");
    
    public static Error IncompatibleSubsonicVersion => new Error(20, "ncompatible Subsonic REST protocol version. Client must upgrade.");
    
    public static Error AuthError => new Error(40, "Wrong username or password.");
    
    public static Error AuthMechanismNotSupportedError => new Error(42, "Provided authentication mechanism not supported.");
    
    public static Error AuthMultipleConflictingProvidedError => new Error(43, "Multiple conflicting authentication mechanisms provided.");
    
    public static Error InvalidApiKeyError => new Error(44, "Invalid API key.");
    
    public static Error UserNotAuthorizedError => new Error(50, "User is not authorized for the given operation.");
    
    public static Error DataNotFoundError => new Error(70, "The requested data was not found.");
}
