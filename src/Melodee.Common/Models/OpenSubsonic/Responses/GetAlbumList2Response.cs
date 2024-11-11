namespace Melodee.Common.Models.OpenSubsonic.Responses;

public sealed record GetAlbumList2Response : ResponseModel<ApiResponse>
{
    public required AlbumList2Wrapper AlbumList2 { get; init; }
}

public sealed record AlbumList2Wrapper
{
    public required AlbumList2[] Album { get; init; }
}
