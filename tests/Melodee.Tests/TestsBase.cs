using Melodee.Common.Configuration;
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

    public static IMelodeeConfiguration NewPluginsConfiguration()
        => new MelodeeConfiguration(NewConfiguration());
    
    public static Dictionary<string, object?> NewConfiguration()
    {
        return MelodeeConfiguration.AllSettings(new Dictionary<string, object?>
        {
            { SettingRegistry.ProcessingAlbumTitleRemovals, "['^', '~', '#']"},
            { SettingRegistry.ProcessingArtistNameReplacements, "{'AC/DC': ['AC; DC', 'AC;DC', 'AC/ DC', 'AC DC'] , 'Love/Hate': ['Love; Hate', 'Love;Hate', 'Love/ Hate', 'Love Hate'] }"},
            { SettingRegistry.ProcessingDoDeleteOriginal, "false" },
            { SettingRegistry.ProcessingDoMoveMelodeeDataFileToStagingDirectory, "true"},            
            { SettingRegistry.ProcessingMaximumAlbumDirectoryNameLength, 255},
            { SettingRegistry.ProcessingMaximumArtistDirectoryNameLength, 255 },
            { SettingRegistry.ProcessingSongTitleRemovals, "[';', '(Remaster)', 'Remaster']"},
            { SettingRegistry.ScriptingPreDiscoveryScript, "/home/steven/incoming/melodee_test/scripts/PreDiscoveryWrapper.sh" },
            { SettingRegistry.ValidationMaximumAlbumYear, 2035},
            { SettingRegistry.ValidationMaximumMediaNumber, 99},
            { SettingRegistry.ValidationMaximumSongNumber, 999},
            { SettingRegistry.ValidationMinimumAlbumYear, 1860}
        });
    }
}
