using System.Net.Sockets;
using Melodee.Common.Models.Configuration;

namespace Melodee.Tests;

public abstract class TestsBase
{
    public static Configuration NewConfiguration => new Configuration
    {
        MediaConvertorOptions = new MediaConvertorOptions(),
        PluginProcessOptions = new PluginProcessOptions
        {
            DoDeleteOriginal = false
        },
        Scripting = new Scripting
        {
            PreDiscoveryScript = "/home/steven/incoming/melodee_test/scripts/PreDiscoveryWrapper.sh"
        },
        InboundDirectory = @"/home/steven/incoming/melodee_test/tests",
        StagingDirectory = @"/home/steven/incoming/melodee_test/staging",
        LibraryDirectory = string.Empty
    };
}