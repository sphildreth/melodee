using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Moq;
using NodaTime;
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
    
    public static OperationResult<Library> TestStagingLibrary()
    {
        return new OperationResult<Library>
        {
            Data = TestLibraries().Data.First(x => x.TypeValue == LibraryType.Staging)
        };
    }    

    public static OperationResult<Library> TestLibrary()
    {
        return new OperationResult<Library>
        {
            Data = TestLibraries().Data.First(x => x.TypeValue == LibraryType.Library)
        };
    }

    public static PagedResult<Library> TestLibraries()
    {
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        return new PagedResult<Library>
        {
            Data =
            [
                new Library
                {
                    Name = "Inbound",
                    Path = "/melodee_test/inbound",
                    Type = SafeParser.ToNumber<int>(LibraryType.Inbound),
                    CreatedAt = now
                },
                new Library
                {
                    Name = "Library",
                    Path = "/melodee_test/library",
                    Type = SafeParser.ToNumber<int>(LibraryType.Library),
                    CreatedAt = now
                },
                new Library
                {
                    Name = "Staging",
                    Path = "/melodee_test/staging",
                    Type = SafeParser.ToNumber<int>(LibraryType.Staging),
                    CreatedAt = now
                }
            ]
        };
    }

    public static Dictionary<string, object?> NewConfiguration()
    {
        return MelodeeConfiguration.AllSettings(new Dictionary<string, object?>
        {
            { SettingRegistry.ProcessingAlbumTitleRemovals, "['^', '~', '#']" },
            { SettingRegistry.ProcessingArtistNameReplacements, "{'AC/DC': ['AC; DC', 'AC;DC', 'AC/ DC', 'AC DC'] , 'Love/Hate': ['Love; Hate', 'Love;Hate', 'Love/ Hate', 'Love Hate'] }" },
            { SettingRegistry.EncryptionPrivateKey, "H+Kiik6VMKfTD2MesF1GoMjczTrD5RhuKckJ5+/UQWOdWajGcsEC3yEnlJ5eoy8Y"},
            { SettingRegistry.OpenSubsonicServerSupportedVersion, "1.16.1"},
            { SettingRegistry.OpenSubsonicServerType, "Melodee"},
            { SettingRegistry.OpenSubsonicServerVersion, "0.1.1"},
            { SettingRegistry.PluginEnabledCueSheet, "true"},
            { SettingRegistry.PluginEnabledSimpleFileVerification, "true"},
            { SettingRegistry.PluginEnabledM3u, "true"},
            { SettingRegistry.PluginEnabledNfo, "true"},
            { SettingRegistry.ProcessingDoDeleteOriginal, "false" },
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
