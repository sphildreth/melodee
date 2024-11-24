namespace Melodee.Common.Models.OpenSubsonic;

public abstract record InfoBase(
    string? SmallImageUrl = null,
    string? MediumImageUrl = null,
    string? LargeImageUrl = null
);
