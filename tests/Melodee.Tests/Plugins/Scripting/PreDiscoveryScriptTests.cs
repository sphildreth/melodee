using Melodee.Common.Constants;
using Melodee.Common.Models;
using Melodee.Plugins.Scripting;

namespace Melodee.Tests.Plugins.Scripting;

public class PreDiscoveryScriptTests
{
    [Fact]
    public async Task ValidatePreDiscoveryScripTestsDisabled()
    {
        var testFile = @"/melodee_test/inbound/00-k 2024";
        var dirInfo = new DirectoryInfo(testFile);
        if (dirInfo.Exists)
        {
            var config = TestsBase.NewPluginsConfiguration();
            config.Configuration[SettingRegistry.ProcessingDoDeleteOriginal] = true;
            var convertor = new PreDiscoveryScript(config);
            var convertorResult = await convertor.ProcessAsync(new FileSystemDirectoryInfo
            {
                Path = dirInfo.FullName,
                Name = dirInfo.Name
            });
            Assert.NotNull(convertorResult);
            Assert.True(convertorResult.IsSuccess);
            Assert.True(convertorResult.Data);
        }
    }

    [Fact]
    public async Task ValidatePreDiscoveryScripTests()
    {
        var testFile = @"/melodee_test/inbound/00-k 2024";
        var testScriptFile = @"/melodee_test/scripts/PreDiscoveryWrapper.sh";
        var dirInfo = new DirectoryInfo(testFile);
        if (dirInfo.Exists && File.Exists(testScriptFile))
        {
            var testNzbFile = Path.Combine(dirInfo.FullName, $"{Guid.NewGuid()}.nzb");
            File.CreateText(testNzbFile);
            Assert.True(File.Exists(testNzbFile));
            var config = TestsBase.NewPluginsConfiguration();
            config.Configuration[SettingRegistry.ProcessingDoDeleteOriginal] = true;
            var scriptRunner = new PreDiscoveryScript(config);
            var convertorResult = await scriptRunner.ProcessAsync(new FileSystemDirectoryInfo
            {
                Path = dirInfo.FullName,
                Name = dirInfo.Name
            });
            Assert.NotNull(convertorResult);
            Assert.True(convertorResult.IsSuccess);
            Assert.True(convertorResult.Data);
            Assert.False(File.Exists(testNzbFile));
        }
    }
}
