using NodaTime;

namespace Melodee.Common.Extensions;

public static class InstantExtensions
{
    public static string ToEtag(this Instant instant) => instant.ToUnixTimeTicks().ToString();
}