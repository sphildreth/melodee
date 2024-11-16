using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Serialization;
using Moq;
using Serilog;

namespace Melodee.Tests;

public abstract class TestsBase
{
    public TestsBase()
    {
        Logger = new Mock<ILogger>().Object;
        Serializer = new Serializer(Logger);
    }

    private ILogger Logger { get; }

    protected ISerializer Serializer { get; }

    public static IMelodeeConfiguration NewPluginsConfiguration()
    {
        return new MelodeeConfiguration(NewConfiguration());
    }

    public static Dictionary<string, object?> NewConfiguration()
    {
        return MelodeeConfiguration.AllSettings(new Dictionary<string, object?>
        {
            { SettingRegistry.ProcessingAlbumTitleRemovals, "['^', '~', '#']" },
            { SettingRegistry.ProcessingArtistNameReplacements, "{'AC/DC': ['AC; DC', 'AC;DC', 'AC/ DC', 'AC DC'] , 'Love/Hate': ['Love; Hate', 'Love;Hate', 'Love/ Hate', 'Love Hate'] }" },
            { SettingRegistry.DirectoryInbound, "/melodee_test/inbound/" },
            { SettingRegistry.DirectoryStaging, "/melodee_test/staging/" },
            { SettingRegistry.DirectoryLibrary, "/melodee_test/library/" },
            { SettingRegistry.EncryptionPrivateKey, "H+Kiik6VMKfTD2MesF1GoMjczTrD5RhuKckJ5+/UQWOdWajGcsEC3yEnlJ5eoy8Y"},
            { SettingRegistry.ProcessingDoDeleteOriginal, "false" },
            { SettingRegistry.ProcessingDoMoveMelodeeDataFileToStagingDirectory, "false" },
            { SettingRegistry.ProcessingMaximumAlbumDirectoryNameLength, 255 },
            { SettingRegistry.ProcessingMaximumArtistDirectoryNameLength, 255 },
            { SettingRegistry.ProcessingSongTitleRemovals, "[';', '(Remaster)', 'Remaster']" },
            { SettingRegistry.ScriptingPreDiscoveryScript, "/melodee_test/scripts/PreDiscoveryWrapper.sh" },
            { SettingRegistry.ValidationMaximumAlbumYear, 2035 },
            { SettingRegistry.ValidationMaximumMediaNumber, 99 },
            { SettingRegistry.ValidationMaximumSongNumber, 999 },
            { SettingRegistry.ValidationMinimumAlbumYear, 1860 }
        });
    }
}
