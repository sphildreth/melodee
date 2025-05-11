using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic.Requests;

/// <summary>
///     Request model for a stream and download request
/// </summary>
/// <param name="Id">A string which uniquely identifies the file to stream. Obtained by calls to getMusicDirectory.</param>
/// <param name="MaxBitRate">
///     If specified, the server will attempt to limit the bitrate to this value, in kilobits per
///     second. If set to zero, no limit is imposed.
/// </param>
/// <param name="Format">
///     Specifies the preferred target format (e.g., “mp3” or “flv”) in case there are multiple applicable
///     transcodings. Starting with 1.9.0 you can use the special value “raw” to disable transcoding.
/// </param>
/// <param name="TimeOffset">
///     By default only applicable to video streaming. If specified, start streaming at the given
///     offset (in seconds) into the media. The Transcode Offset extension enables the parameter to music too.
/// </param>
public record StreamRequest(string Id, int? MaxBitRate, string? Format, int? TimeOffset)
{
    public bool IsDownloadingRequest { get; set; }

    public bool IsRawFormat => Format.Nullify() != null && Format.ToNormalizedString() == "RAW";

    public bool IsTranscodingRequest =>
        !IsDownloadingRequest && !IsRawFormat && MaxBitRate != null && Format.Nullify() != null;
}
