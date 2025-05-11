using Melodee.Common.Enums;

namespace Melodee.Common.Extensions;

public static class ContributorTypeExtensions
{
    public static bool RestrictToOnePerAlbum(this ContributorType type)
    {
        return type == ContributorType.Publisher;
    }
}
