namespace Melodee.Common.Models.OpenSubsonic.Responses;

public sealed record GetAlbumList2Response : ResponseModel<ApiResponse>
{
    public required AlbumId3WithSongs[] AlbumList2 { get; init; }
}
