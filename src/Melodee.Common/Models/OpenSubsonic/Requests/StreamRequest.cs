namespace Melodee.Common.Models.OpenSubsonic.Requests;

public record StreamRequest(string Id, int? MaxRate, string? Format, int? TimeOffset, bool? EstimateContentLength, bool? Converted);
