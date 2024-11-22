using Microsoft.Extensions.Primitives;

namespace Melodee.Common.Models.OpenSubsonic.Responses;

public sealed record StreamResponse(IDictionary<string, StringValues> ResponseHeaders, bool IsSuccess, byte[] Bytes);
