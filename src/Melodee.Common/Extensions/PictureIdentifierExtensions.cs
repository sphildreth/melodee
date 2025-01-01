using Melodee.Common.Enums;

namespace Melodee.Common.Extensions;

public static class PictureIdentifierExtensions
{
    /// <summary>
    ///     Should the validator check if the image is square for the given image identifier.
    /// </summary>
    public static bool ValidateIsSquare(this PictureIdentifier identifier)
    {
        switch (identifier)
        {
            case PictureIdentifier.Front:
            case PictureIdentifier.SecondaryFront:
            case PictureIdentifier.Artist:
            case PictureIdentifier.ArtistSecondary:
                return true;
        }

        return false;
    }
}
