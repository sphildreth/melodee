namespace Melodee.Common.Models.OpenSubsonic;

public record Error(short Code, string Message, string? HelpUrl = null)
{
    public static Error RequiredParameterMissingError => new(10, "Required parameter is missing.");

    public static Error IncompatibleSubsonicVersion => new(20, "Incompatible Subsonic REST protocol version. Client must upgrade.");

    public static Error AuthError => new(40, "Wrong username or password.");

    public static Error AuthMechanismNotSupportedError => new(42, "Provided authentication mechanism not supported.");

    public static Error AuthMultipleConflictingProvidedError => new(43, "Multiple conflicting authentication mechanisms provided.");

    public static Error InvalidApiKeyError => new(44, "Invalid API key.");

    public static Error UserNotAuthorizedError => new(50, "User is not authorized for the given operation.");

    public static Error DataNotFoundError => new(70, "The requested data was not found.");

    public static Error GenericError(string? message)
    {
        return new Error(0, message ?? "An error has occured.");
    }
}
