using Melodee.Common.Constants;
using Melodee.Common.Extensions;

namespace Melodee.Tests.Extensions;

public class TypeExtensions
{
    [Fact]
    public void GetAllSettingsRegistryConsts()
    {
        var allConsts = typeof(SettingRegistry).GetAllPublicConstantValues<string>();
        Assert.NotNull(allConsts);
        Assert.Contains(allConsts, x => x == SettingRegistry.DirectoryLibrary);
    }
}
