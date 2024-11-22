namespace Melodee.Common.Models.OpenSubsonic.Requests;

public record StreamRequest(Guid Id, int? MaxRate, string? Format, int? TimeOffset, bool? EstimateContentLength, bool? Converted);
