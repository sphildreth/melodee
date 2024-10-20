namespace Melodee.Common.Models.Configuration;

[Serializable]
public sealed record MediaConvertorOptions
{
    public bool ConversionEnabled { get; set; } = true;

    public int ConvertBitrate { get; set; } = 384;

    public int ConvertVbrLevel { get; set; } = 4;

    public int ConvertSamplingRate { get; set; } = 48000;
}
