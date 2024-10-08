using Melodee.Common.Models;

namespace Melodee.Tests.Plugins.Scripting;

public class PreDiscoveryScriptTests
{
    [Fact] public async Task ValidatePreDiscoveryScripTestsDisabled()
    {
        var testFile = @"/home/steven/incoming/melodee_test/inbound/00-k 2024";
        var dirInfo = new DirectoryInfo(testFile);
        if (dirInfo.Exists)
        {
            var config = TestsBase.NewConfiguration;
            config.PluginProcessOptions.DoDeleteOriginal = true;
            var convertor = new Melodee.Plugins.Scripting.PreDiscoveryScript(config);
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
    
    [Fact] public async Task ValidatePreDiscoveryScripTests()
    {
        var testFile = @"/home/steven/incoming/melodee_test/inbound/00-k 2024";
        var testScriptFile = @"/home/steven/incoming/melodee_test/scripts/PreDiscoveryWrapper.sh";
        var dirInfo = new DirectoryInfo(testFile);
        if (dirInfo.Exists && File.Exists(testScriptFile))
        {
            var testNzbFile = Path.Combine(dirInfo.FullName, $"{Guid.NewGuid()}.nzb");
            File.CreateText(testNzbFile);
            Assert.True(File.Exists(testNzbFile));
            var config = TestsBase.NewConfiguration;
            config.PluginProcessOptions.DoDeleteOriginal = true;
            var scriptRunner = new Melodee.Plugins.Scripting.PreDiscoveryScript(config);
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