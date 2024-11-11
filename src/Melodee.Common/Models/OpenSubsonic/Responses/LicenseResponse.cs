namespace Melodee.Common.Models.OpenSubsonic.Responses;

public sealed record LicenseResponse : ResponseModel<ApiResponse>
{
    public required License License { get; init; }
}
