using Melodee.Common.Models.Configuration;
using Melodee.Plugins.Conversion.Models;

namespace Melodee.Tests.Plugins.Conversion;

public class PreDiscoveryScriptTests
{
    [Fact] public async Task ValidatePreDiscoveryScripTestsDisabled()
    {
        var testFile = @"/home/steven/incoming/melodee_test/inbound/00-k 2024";
        var dirInfo = new System.IO.DirectoryInfo(testFile);
        if (dirInfo.Exists)
        {
            var convertor = new Melodee.Plugins.Scripting.PreDiscoveryScript(new Configuration
                {
                    MediaConvertorOptions = new MediaConvertorOptions(),
                    Scripting = new Scripting(),
                    InboundDirectory = @"/home/steven/incoming/melodee_test/tests",
                    StagingDirectory = string.Empty,
                    LibraryDirectory = string.Empty
                }
            );
            var convertorResult = await convertor.ProcessAsync(dirInfo, new ProcessFileOptions
            {
                DoDeleteOriginal = false
            });
            Assert.NotNull(convertorResult);
            Assert.False(convertorResult.IsSuccess);
            Assert.NotNull(convertorResult.Data);
        }
    }       
    
    [Fact] public async Task ValidatePreDiscoveryScripTests()
    {
        var testFile = @"/home/steven/incoming/melodee_test/inbound/00-k 2024";
        var testScriptFile = @"/home/steven/incoming/melodee_test/scripts/PreDiscoveryWrapper.sh";
        var dirInfo = new System.IO.DirectoryInfo(testFile);
        if (dirInfo.Exists && File.Exists(testScriptFile))
        {
            var testNzbFile = Path.Combine(dirInfo.FullName, $"{Guid.NewGuid()}.nzb");
            File.CreateText(testNzbFile);
            Assert.True(File.Exists(testNzbFile));
            var scriptRunner = new Melodee.Plugins.Scripting.PreDiscoveryScript(new Configuration
                {
                    MediaConvertorOptions = new MediaConvertorOptions(),
                    Scripting = new Scripting
                    {
                        PreDiscoveryScript = "/home/steven/incoming/melodee_test/scripts/PreDiscoveryWrapper.sh"
                    },
                    InboundDirectory = @"/home/steven/incoming/melodee_test/tests",
                    StagingDirectory = string.Empty,
                    LibraryDirectory = string.Empty
                }
            );
            var convertorResult = await scriptRunner.ProcessAsync(dirInfo, new ProcessFileOptions());
            Assert.NotNull(convertorResult);
            Assert.False(convertorResult.IsSuccess);
            Assert.NotNull(convertorResult.Data);
            Assert.False(File.Exists(testNzbFile));
        }
    }        
    
    
}