namespace Melodee.Common.Models.Configuration;

[Serializable]
public sealed record MediaConvertorOptions
{
    public bool ConversionEnabled { get; init; } = true;
    
    public int ConvertBitrate { get; init; } = 384;

    public int ConvertVbrLevel { get; init; } = 4;
    
    public int ConvertSamplingRate { get; init; } = 48000;
}