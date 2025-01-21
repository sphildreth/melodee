using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Tests.Services;
using Melodee.Tests.Validation;

namespace Melodee.Tests.Common;

public class SerializerTests : ServiceTestBase
{
    [Fact]
    public void CanSerializerAndDeserializerAlbum()
    {
        var album = AlbumValidatorTests.TestAlbum;
        var serialized = Serializer.Serialize(album);
        Assert.NotNull(serialized);
        var deserialized = Serializer.Deserialize<Album>(serialized);
        Assert.NotNull(deserialized);
        Assert.Equal(album.Id, deserialized.Id);
        Assert.Equal(album.Status, deserialized.Status);
        Assert.Equal(album.AlbumType, deserialized.AlbumType);
        Assert.NotNull(deserialized.Songs);

        Assert.Equal(AlbumValidatorTests.ShouldBeBitRate, deserialized.Songs?.FirstOrDefault()?.BitRate());
    }
}
