namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
///     A disc title for an album
/// </summary>
/// <param name="Disc">The disc number.</param>
/// <param name="Title">The name of the disc.</param>
public record DiscTitle(
    int Disc,
    string Title
);
