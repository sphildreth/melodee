using Melodee.Common.Constants;

namespace Melodee.Common.Configuration.Extensions;

public static class IMelodeeConfigurationExtensions
{
    public static short DefaultPageSize(this IMelodeeConfiguration configuration)
    {
        return configuration.GetValue<short>(SettingRegistry.DefaultsPageSize);
    }

    public static int[] DefaultPageSizeOptions(this IMelodeeConfiguration configuration)
    {
        var pageSize = configuration.DefaultPageSize();
        return [pageSize, pageSize * 2, pageSize * 5, pageSize * 10];
    }
}
