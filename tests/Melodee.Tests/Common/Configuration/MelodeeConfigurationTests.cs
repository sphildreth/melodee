using Melodee.Common.Constants;
using Melodee.Common.Extensions;

namespace Melodee.Tests.Common.Configuration;

public class MelodeeConfigurationTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("The Outlaws", "Outlaws")]
    [InlineData("El Outlaws", "Outlaws")]
    [InlineData("LOS Outlaws", "Outlaws")]
    [InlineData("A Bad Boy", "Bad Boy")]
    [InlineData("Something with a Series Of LETTERs", "Something with a Series Of LETTERs")]
    [InlineData("Out in the West", "Out in the West")]
    [InlineData("Colin Hay", "Colin Hay")]
    public void ValidateArticlesAreRemoved(string? input, string? shouldBe)
    {
        var configuration = TestsBase.NewPluginsConfiguration();
        configuration.SetSetting(SettingRegistry.ProcessingIgnoredArticles, "THE|EL|LA|LOS|LAS|LE|LES|OS|AS|O|A");
        Assert.Equal(shouldBe, configuration.RemoveUnwantedArticles(input));

        configuration.SetSetting(SettingRegistry.ProcessingIgnoredArticles, string.Empty);
        Assert.Equal(input.Nullify(), configuration.RemoveUnwantedArticles(input));
    }
}
