using System.Diagnostics;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Tests.Validation;
using Moq;
using Serilog;
using Serilog.Core;

namespace Melodee.Tests.Common;

public class SerializerTests
{
    private ILogger Logger { get; }
    
    private ISerializer Serializer { get; }

    public SerializerTests()
    {
        Logger = new Mock<ILogger>().Object;
        Serializer = new Serializer(Logger);
    }
    
    [Fact]
    public void CanSerializerAndDeserializerAlbum()
    {
        var album = AlbumValidatorTests.TestAlbum;
        var serialized = Serializer.Serialize(album);
        Assert.NotNull(serialized);
        var deserialized = Serializer.Deserialize<Album>(serialized);
        Assert.NotNull(deserialized);
        Assert.Equal(album.UniqueId(), deserialized.UniqueId());
        Assert.Equal(album.Status, deserialized.Status);
        Assert.NotNull(deserialized.Songs);

        Assert.Equal(AlbumValidatorTests.ShouldBeBitRate, deserialized.Songs?.FirstOrDefault()?.BitRate());
    }
}
