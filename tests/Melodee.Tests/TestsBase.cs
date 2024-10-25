using Melodee.Common.Constants;
using Melodee.Common.Serialization;
using Melodee.Plugins;
using Melodee.Services;
using Moq;
using Serilog;

namespace Melodee.Tests;

public abstract class TestsBase
{
    ILogger Logger { get; }
    
    protected ISerializer Serializer { get; }

    public TestsBase()
    {
        Logger = new Mock<ILogger>().Object;
        Serializer = new Serializer(Logger);
    }

    public static IPluginsConfiguration NewPluginsConfiguration()
        => new PluginsConfiguration(NewConfiguration());
    
    public static Dictionary<string, object?> NewConfiguration()
    {
        return SettingService.AllSettings(new Dictionary<string, object?>
        {
            { SettingRegistry.ProcessingDoDeleteOriginal, "false" },
            { SettingRegistry.ScriptingPreDiscoveryScript, "/home/steven/incoming/melodee_test/scripts/PreDiscoveryWrapper.sh" },
            { SettingRegistry.DirectoryInbound, @"/home/steven/incoming/melodee_test/tests" },
            { SettingRegistry.DirectoryStaging, @"/home/steven/incoming/melodee_test/staging" },
            { SettingRegistry.DirectoryLibrary, string.Empty }
        });
    }
}
