namespace Melodee.Common.Models.OpenSubsonic.Responses;

public sealed record GetGenresResponse : ResponseModel<ApiResponse>
{
    public required GetGenresResponseWrapper Genres { get; init; }
}

public sealed record GetGenresResponseWrapper
{
    public required Genre[] Genre { get; init; }
}
